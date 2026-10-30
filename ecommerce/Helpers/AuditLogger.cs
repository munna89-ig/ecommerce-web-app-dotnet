using System.Security.Claims;
using System.Text.Json;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Helpers
{
    public interface IAuditLogger
    {
        Task LogAsync(
            string action,
            string entityType,
            int? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            int? userId = null);
    }

    // For events that do not change a table: login, logout, failed login ...
    public class AuditLogger : IAuditLogger
    {
        private readonly EcommerceSystemContext _context;
        private readonly IHttpContextAccessor _http;

        public AuditLogger(EcommerceSystemContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync(
            string action,
            string entityType,
            int? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            int? userId = null)
        {
            var http = _http.HttpContext;

            if (userId == null &&
                int.TryParse(http?.User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
            {
                userId = id;
            }

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues == null ? null : JsonSerializer.Serialize(newValues),
                IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}