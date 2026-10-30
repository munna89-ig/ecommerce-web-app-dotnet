using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Helpers
{
    // Writes an audit_logs row for every insert / update / delete that goes through SaveChanges.
    public class AuditInterceptor : SaveChangesInterceptor
    {
        // tables that change too often / are already logs => not audited
        private static readonly HashSet<Type> SkipTypes = new()
        {
            typeof(AuditLog),
            typeof(CartItem),
            typeof(GuestUser),
            typeof(Wishlist),
            typeof(OrderItem),
            typeof(OrderStatusHistory),
            typeof(ShippingAddress)
        };

        // never written to the log in plain text
        private static readonly HashSet<string> HiddenProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash"
        };

        // noise: changes on every save
        private static readonly HashSet<string> IgnoredProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedAt", "UpdatedAt"
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly IHttpContextAccessor _http;

        private List<PendingLog> _pending = new();
        private bool _writingLogs;

        public AuditInterceptor(IHttpContextAccessor http)
        {
            _http = http;
        }

        // ---------------------------------------------------------
        // before save: remember what is going to change
        // ---------------------------------------------------------
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            Capture(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Capture(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // ---------------------------------------------------------
        // after save: new ids are known => write the logs
        // ---------------------------------------------------------
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            WriteLogsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            await WriteLogsAsync(eventData.Context, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            _pending.Clear();
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            _pending.Clear();
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        // ---------------------------------------------------------
        private void Capture(DbContext? context)
        {
            _pending = new List<PendingLog>();

            if (context == null || _writingLogs) return;

            var entries = context.ChangeTracker.Entries().ToList();

            // the code already wrote a hand-made (more meaningful) log in this save
            if (entries.Any(e => e.Entity is AuditLog && e.State == EntityState.Added))
                return;

            foreach (var entry in entries)
            {
                // many-to-many join rows (coupon_categories ...) and skipped tables
                if (entry.Entity is Dictionary<string, object>) continue;
                if (SkipTypes.Contains(entry.Metadata.ClrType)) continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        _pending.Add(new PendingLog(entry, "create", null, null, AllValues(entry, original: false)));
                        break;

                    case EntityState.Deleted:
                        _pending.Add(new PendingLog(entry, "delete", KeyOf(entry), AllValues(entry, original: true), null));
                        break;

                    case EntityState.Modified:
                        var changed = entry.Properties
                            .Where(p => p.IsModified &&
                                        !IgnoredProps.Contains(p.Metadata.Name) &&
                                        !Equals(p.OriginalValue, p.CurrentValue))
                            .ToList();

                        if (changed.Count == 0) continue;

                        // soft delete (DeletedAt set) counts as a delete
                        bool softDelete = changed.Any(p =>
                            p.Metadata.Name == "DeletedAt" && p.OriginalValue == null && p.CurrentValue != null);

                        _pending.Add(new PendingLog(
                            entry,
                            softDelete ? "delete" : "update",
                            KeyOf(entry),
                            changed.ToDictionary(p => p.Metadata.Name, p => Mask(p.Metadata.Name, p.OriginalValue)),
                            changed.ToDictionary(p => p.Metadata.Name, p => Mask(p.Metadata.Name, p.CurrentValue))));
                        break;
                }
            }
        }

        private async Task WriteLogsAsync(DbContext? context, CancellationToken ct)
        {
            if (context == null || _pending.Count == 0) return;

            var http = _http.HttpContext;
            int? userId = int.TryParse(http?.User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
            string? ip = http?.Connection.RemoteIpAddress?.ToString();
            var now = DateTime.UtcNow;

            foreach (var p in _pending)
            {
                string typeName = Snake(p.Entry.Metadata.ClrType.Name);   // "Product" => "product"

                context.Set<AuditLog>().Add(new AuditLog
                {
                    UserId = userId,
                    Action = $"{p.Verb}_{typeName}",                          // create_product
                    EntityType = p.Entry.Metadata.GetTableName() ?? typeName, // products
                    EntityId = p.EntityId ?? KeyOf(p.Entry),                  // new rows: id known now
                    OldValues = p.Old == null ? null : JsonSerializer.Serialize(p.Old, JsonOptions),
                    NewValues = p.New == null ? null : JsonSerializer.Serialize(p.New, JsonOptions),
                    IpAddress = ip,
                    CreatedAt = now
                });
            }

            _pending = new List<PendingLog>();

            _writingLogs = true;
            try
            {
                await context.SaveChangesAsync(ct);
            }
            finally
            {
                _writingLogs = false;
            }
        }

        // ---------------------------------------------------------
        private static Dictionary<string, object?> AllValues(EntityEntry entry, bool original)
        {
            return entry.Properties
                .Where(p => !p.Metadata.IsPrimaryKey() && !IgnoredProps.Contains(p.Metadata.Name))
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => Mask(p.Metadata.Name, original ? p.OriginalValue : p.CurrentValue));
        }

        private static object? Mask(string property, object? value)
            => HiddenProps.Contains(property) && value != null ? "***" : value;

        private static int? KeyOf(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key == null || key.Properties.Count != 1) return null;

            var value = entry.Property(key.Properties[0].Name).CurrentValue;
            return value is int i && i > 0 ? i : null;
        }

        private static string Snake(string name)
            => Regex.Replace(name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant();

        private sealed record PendingLog(
            EntityEntry Entry,
            string Verb,
            int? EntityId,
            Dictionary<string, object?>? Old,
            Dictionary<string, object?>? New);
    }
}