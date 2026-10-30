using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Interfaces;
using Pearl_Poetry.ViewModels.Order;

namespace Pearl_Poetry.Services
{
    public class OrderService : IOrderService
    {
        private readonly EcommerceSystemContext _context;

        public OrderService(EcommerceSystemContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET ORDERS
        // =========================================================

        public async Task<(List<OrderListViewModel> Orders, int TotalItems)> GetOrdersAsync(
            int page,
            int pageSize,
            string? search = null,
            string? status = null,
            string? paymentStatus = null)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;


            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.GuestUser)
                .AsQueryable();


            // =========================
            // SEARCH
            // =========================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    o.ShipFullName.Contains(search) ||
                    o.ShipPhone.Contains(search) ||
                    o.ShipEmail.Contains(search));
            }


            // =========================
            // ORDER STATUS
            // =========================

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status.ToLower());
            }


            // =========================
            // PAYMENT STATUS
            // =========================

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                query = query.Where(o =>
                    o.PaymentStatus == paymentStatus.ToLower());
            }


            int totalItems = await query.CountAsync();


            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,

                    OrderNumber = o.OrderNumber,

                    CustomerName =
                        o.User != null
                            ? o.User.Name
                            : o.GuestUser != null
                                ? o.GuestUser.Name
                                : o.ShipFullName,

                    CustomerEmail =
                        o.User != null
                            ? o.User.Email
                            : o.GuestUser != null
                                ? o.GuestUser.Email ?? ""
                                : o.ShipEmail,

                    CustomerPhone =
                        o.User != null
                            ? o.User.Phone ?? ""
                            : o.GuestUser != null
                                ? o.GuestUser.Phone ?? ""
                                : o.ShipPhone,

                    Subtotal = (decimal)o.Subtotal,

                    DiscountAmount = (decimal)o.DiscountAmount,

                    DeliveryCharge = (decimal)o.DeliveryCharge,

                    TotalAmount = o.TotalAmount,

                    Status = o.Status,

                    PaymentStatus = o.PaymentStatus,

                    CreatedAt = (DateTime)o.CreatedAt
                })
                .ToListAsync();


            return (orders, totalItems);
        }


        // =========================================================
        // GET ORDER DETAILS
        // =========================================================

        public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.GuestUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Variant)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.VerifiedByNavigation)
                .Include(o => o.OrderStatusHistories)
                    .ThenInclude(h => h.ChangedByNavigation)
                .FirstOrDefaultAsync(o => o.Id == id);


            if (order == null)
                return null;


            var model = new OrderDetailsViewModel
            {
                Id = order.Id,

                OrderNumber = order.OrderNumber,

                Status = order.Status,

                PaymentStatus = order.PaymentStatus,

                CreatedAt = (DateTime)order.CreatedAt,

                UpdatedAt = order.UpdatedAt,

                CustomerNote = order.CustomerNote,

                CouponCode = order.CouponCode,

                UserId = order.UserId,


                // Customer
                CustomerName =
                    order.User?.Name
                    ?? order.GuestUser?.Name
                    ?? order.ShipFullName,

                CustomerEmail =
                    order.User?.Email
                    ?? order.GuestUser?.Email
                    ?? order.ShipEmail
                    ?? "",

                CustomerPhone =
                    order.User?.Phone
                    ?? order.GuestUser?.Phone
                    ?? order.ShipPhone
                    ?? "",


                // Shipping
                ShippingFullName = order.ShipFullName,

                ShippingPhone = order.ShipPhone,

                ShippingEmail = order.ShipEmail,

                ShippingAddress = order.ShipAddress,

                ShippingDistrict = order.ShipDistrict,

                ShippingPostalCode = order.ShipPostalCode,

                ShippingCountry = order.ShipCountry,

                ShippingDeliveryNote = order.ShipDeliveryNote,


                // Price
                Subtotal = (decimal)order.Subtotal,

                DiscountAmount = (decimal)order.DiscountAmount,

                DeliveryCharge = (decimal)order.DeliveryCharge,

                TotalAmount = order.TotalAmount,


                // Available statuses
                AvailableStatuses = Enum
                    .GetNames<OrderStatus>()
                    .Select(x => x.ToLower())
                    .ToList()
            };


            // =====================================================
            // ITEMS
            // =====================================================

            model.Items = order.OrderItems
                .Select(item => new OrderItemViewModel
                {
                    Id = item.Id,

                    ProductId = item.ProductId,

                    VariantId = item.VariantId,

                    ProductTitle = item.ProductTitle,

                    VariantLabel = item.VariantLabel,

                    Price = item.Price,

                    Quantity = item.Quantity
                })
                .ToList();


            // =====================================================
            // PAYMENTS
            // =====================================================

            model.Payments = order.Payments
                .OrderByDescending(p => p.CreatedAt)
                .Select(payment => new OrderPaymentViewModel
                {
                    Id = payment.Id,

                    Amount = payment.Amount,

                    PaymentMethod = payment.PaymentMethod,

                    Status = payment.Status,

                    TransactionId = payment.TransactionId,

                    SenderName = payment.SenderName,

                    SenderPhone = payment.SenderPhone,

                    VerificationNote = payment.VerificationNote,

                    CreatedAt = (DateTime)payment.CreatedAt,

                    PaidAt = payment.PaidAt,

                    VerifiedByName =
                        payment.VerifiedByNavigation?.Name
                })
                .ToList();


            // =====================================================
            // STATUS HISTORY
            // =====================================================

            model.StatusHistory = order.OrderStatusHistories
                .OrderByDescending(h => h.CreatedAt)
                .Select(history => new OrderStatusHistoryViewModel
                {
                    Id = history.Id,

                    Status = history.Status,

                    Note = history.Note,

                    CreatedAt = (DateTime)history.CreatedAt,

                    ChangedByName =
                        history.ChangedByNavigation?.Name
                })
                .ToList();


            return model;
        }


        // =========================================================
        // UPDATE ORDER STATUS
        // =========================================================

        public async Task<bool> UpdateOrderStatusAsync(
            int orderId,
            OrderStatus newStatus,
            int? changedBy,
            string? note)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);


            if (order == null)
                return false;


            string newStatusValue =
                newStatus.ToString().ToLower();


            string oldStatus = order.Status;


            // No change
            if (oldStatus == newStatusValue)
                return true;


            order.Status = newStatusValue;

            order.UpdatedAt = DateTime.UtcNow;


            // =====================================================
            // STATUS HISTORY
            // =====================================================

            var history = new OrderStatusHistory
            {
                OrderId = order.Id,

                Status = newStatusValue,

                ChangedBy = changedBy,

                Note = note,

                CreatedAt = DateTime.UtcNow
            };


            _context.OrderStatusHistories.Add(history);


            await _context.SaveChangesAsync();


            return true;
        }


        // =========================================================
        // UPDATE PAYMENT STATUS
        // =========================================================

        public async Task<bool> UpdatePaymentStatusAsync(
            int orderId,
            string paymentStatus)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);


            if (order == null)
                return false;


            order.PaymentStatus =
                paymentStatus.Trim().ToLower();

            order.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();


            return true;
        }
    }
}