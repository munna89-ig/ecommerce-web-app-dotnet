using Pearl_Poetry.Enums;
using Pearl_Poetry.ViewModels.Order;

namespace Pearl_Poetry.Interfaces
{
    public interface IOrderService
    {
        Task<(List<OrderListViewModel> Orders, int TotalItems)> GetOrdersAsync(
            int page,
            int pageSize,
            string? search = null,
            string? status = null,
            string? paymentStatus = null);

        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int id);

        Task<bool> UpdateOrderStatusAsync(
            int orderId,
            OrderStatus newStatus,
            int? changedBy,
            string? note);

        Task<bool> UpdatePaymentStatusAsync(
            int orderId,
            string paymentStatus);
    }
}