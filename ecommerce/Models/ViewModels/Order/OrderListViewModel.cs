using Pearl_Poetry.Enums;

namespace Pearl_Poetry.ViewModels.Order
{
    public class OrderListViewModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = "Guest";

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal DeliveryCharge { get; set; }

        public string Status { get; set; } = "pending";

        public string PaymentStatus { get; set; } = "unpaid";

        public DateTime CreatedAt { get; set; }
    }
}