namespace Pearl_Poetry.ViewModels.Order
{
    public class OrderDetailsViewModel
    {
        // =========================
        // ORDER
        // =========================

        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? CustomerNote { get; set; }

        public string? CouponCode { get; set; }


        // =========================
        // CUSTOMER
        // =========================

        public int? UserId { get; set; }

        public string CustomerName { get; set; } = "Guest";

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;


        // =========================
        // SHIPPING
        // =========================

        public string ShippingFullName { get; set; } = string.Empty;

        public string ShippingPhone { get; set; } = string.Empty;

        public string ShippingEmail { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string ShippingDistrict { get; set; } = string.Empty;

        public string ShippingPostalCode { get; set; } = string.Empty;

        public string ShippingCountry { get; set; } = "Bangladesh";

        public string? ShippingDeliveryNote { get; set; }


        // =========================
        // PRICE
        // =========================

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal DeliveryCharge { get; set; }

        public decimal TotalAmount { get; set; }


        // =========================
        // ORDER ITEMS
        // =========================

        public List<OrderItemViewModel> Items { get; set; } = new();


        // =========================
        // PAYMENTS
        // =========================

        public List<OrderPaymentViewModel> Payments { get; set; } = new();


        // =========================
        // STATUS HISTORY
        // =========================

        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new();


        // =========================
        // AVAILABLE STATUSES
        // =========================

        public List<string> AvailableStatuses { get; set; } = new();
    }


    public class OrderItemViewModel
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public int? VariantId { get; set; }

        public string ProductTitle { get; set; } = string.Empty;

        public string? VariantLabel { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal => Price * Quantity;
    }


    public class OrderPaymentViewModel
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public string? SenderName { get; set; }

        public string? SenderPhone { get; set; }

        public string? VerificationNote { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? VerifiedByName { get; set; }
    }


    public class OrderStatusHistoryViewModel
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ChangedByName { get; set; }
    }
}