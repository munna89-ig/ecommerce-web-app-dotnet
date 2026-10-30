using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Admin.Payment;
using System.Text.Json;

namespace Pearl_Poetry.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly EcommerceSystemContext _context;

        public PaymentService(EcommerceSystemContext context)
        {
            _context = context;
        }

        // =========================================================
        // LIST
        // =========================================================
        public async Task<PaymentListPageViewModel> GetPaymentsAsync(
            string? search, string? status, string? method, int page, int pageSize)
        {
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            status = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLower();
            method = string.IsNullOrWhiteSpace(method) ? null : method.Trim().ToLower();
            if (page < 1) page = 1;

            var query = _context.Payments.AsNoTracking().AsQueryable();

            if (search != null)
            {
                query = query.Where(p =>
                    p.Order.OrderNumber.Contains(search) ||
                    p.Order.ShipFullName.Contains(search) ||
                    p.Order.ShipPhone.Contains(search) ||
                    (p.TransactionId != null && p.TransactionId.Contains(search)) ||
                    (p.SenderPhone != null && p.SenderPhone.Contains(search)) ||
                    (p.SenderName != null && p.SenderName.Contains(search)));
            }

            if (status != null)
                query = query.Where(p => p.Status == status);

            if (method != null)
                query = query.Where(p => p.PaymentMethod == method);

            int totalItems = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            if (page > totalPages) page = totalPages;

            var rows = await query
                // payments waiting for a decision first, newest first
                .OrderBy(p => p.Status == "pending_verification" ? 0 : 1)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentRowViewModel
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderNumber = p.Order.OrderNumber,
                    OrderTotal = p.Order.TotalAmount,
                    OrderStatus = p.Order.Status,
                    CustomerName = p.User != null ? p.User.Name : p.Order.ShipFullName,
                    CustomerPhone = p.Order.ShipPhone,
                    IsGuest = p.UserId == null,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    SenderName = p.SenderName,
                    SenderPhone = p.SenderPhone,
                    Amount = p.Amount,
                    Status = p.Status,
                    VerificationNote = p.VerificationNote,
                    VerifiedByName = p.VerifiedByNavigation != null ? p.VerifiedByNavigation.Name : null,
                    CreatedAt = p.CreatedAt,
                    PaidAt = p.PaidAt
                })
                .ToListAsync();

            // ---------- summary cards (not affected by the filters) ----------
            var all = _context.Payments.AsNoTracking();

            var model = new PaymentListPageViewModel
            {
                Payments = rows,
                Search = search,
                Status = status,
                Method = method,
                Page = page,
                TotalPages = totalPages,
                TotalItems = totalItems,

                PendingCount = await all.CountAsync(p => p.Status == "pending_verification"),
                PendingAmount = await all.Where(p => p.Status == "pending_verification")
                                         .SumAsync(p => (decimal?)p.Amount) ?? 0m,
                VerifiedAmount = await all.Where(p => p.Status == "paid")
                                          .SumAsync(p => (decimal?)p.Amount) ?? 0m,
                RejectedCount = await all.CountAsync(p => p.Status == "failed"),

                // cash-on-delivery orders: no payment row, still unpaid, not cancelled
                CodUnpaidOrders = await _context.Orders.CountAsync(o =>
                    !o.Payments.Any() &&
                    o.PaymentStatus == "unpaid" &&
                    o.Status != "cancelled" && o.Status != "refunded"),

                Methods = await all.Select(p => p.PaymentMethod).Distinct().OrderBy(m => m).ToListAsync()
            };

            return model;
        }

        // =========================================================
        // VERIFY
        // =========================================================
        public async Task<(bool Success, string Message)> VerifyAsync(
            int paymentId, int? adminId, string? note, string? ipAddress)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return (false, "Payment not found.");

            if (payment.Status == "paid")
                return (false, "This payment is already verified.");

            var now = DateTime.UtcNow;
            var order = payment.Order;
            string oldStatus = payment.Status;

            payment.Status = "paid";
            payment.VerifiedBy = adminId;
            payment.PaidAt = now;
            payment.VerificationNote = Clean(note);

            order.PaymentStatus = "paid";
            order.UpdatedAt = now;

            // a pending order is confirmed as soon as its money is confirmed
            if (order.Status == "pending")
            {
                order.Status = "confirmed";
                order.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    Status = "confirmed",
                    Note = $"Payment verified ({payment.PaymentMethod}" +
                           (string.IsNullOrWhiteSpace(payment.TransactionId) ? ")" : $", TrxID {payment.TransactionId})"),
                    ChangedBy = adminId,
                    CreatedAt = now
                });
            }

            AddAudit(adminId, "payment_verified", payment.Id, ipAddress,
                new { status = oldStatus },
                new { status = "paid", order = order.OrderNumber, note = payment.VerificationNote });

            await _context.SaveChangesAsync();

            return (true, $"Payment for order #{order.OrderNumber} was verified.");
        }

        // =========================================================
        // REJECT
        // =========================================================
        public async Task<(bool Success, string Message)> RejectAsync(
            int paymentId, int? adminId, string? note, string? ipAddress)
        {
            note = Clean(note);
            if (note == null)
                return (false, "Please write why the payment is rejected.");

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return (false, "Payment not found.");

            if (payment.Status == "failed")
                return (false, "This payment is already rejected.");

            var now = DateTime.UtcNow;
            var order = payment.Order;
            string oldStatus = payment.Status;

            payment.Status = "failed";
            payment.VerifiedBy = adminId;
            payment.PaidAt = null;
            payment.VerificationNote = note;

            order.PaymentStatus = "failed";
            order.UpdatedAt = now;

            AddAudit(adminId, "payment_rejected", payment.Id, ipAddress,
                new { status = oldStatus },
                new { status = "failed", order = order.OrderNumber, note });

            await _context.SaveChangesAsync();

            return (true, $"Payment for order #{order.OrderNumber} was rejected.");
        }

        // =========================================================
        // PRIVATE
        // =========================================================
        private void AddAudit(int? adminId, string action, int entityId, string? ip, object oldValues, object newValues)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminId,
                Action = action,
                EntityType = "payment",
                EntityId = entityId,
                OldValues = JsonSerializer.Serialize(oldValues),
                NewValues = JsonSerializer.Serialize(newValues),
                IpAddress = ip,
                CreatedAt = DateTime.UtcNow
            });
        }

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}