namespace Pearl_Poetry.ViewModels.Admin.Payment
{
    public class PaymentListPageViewModel
    {
        public List<PaymentRowViewModel> Payments { get; set; } = new();

        // ---------- filters ----------
        public string? Search { get; set; }

        public string? Status { get; set; }

        public string? Method { get; set; }

        // ---------- paging ----------
        public int Page { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public int TotalItems { get; set; }

        // ---------- summary cards ----------
        public int PendingCount { get; set; }

        public decimal PendingAmount { get; set; }

        public decimal VerifiedAmount { get; set; }

        public int RejectedCount { get; set; }

        public int CodUnpaidOrders { get; set; }

        public List<string> Methods { get; set; } = new();
    }

    public class PaymentRowViewModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public decimal OrderTotal { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public bool IsGuest { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public string? SenderName { get; set; }

        public string? SenderPhone { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? VerificationNote { get; set; }

        public string? VerifiedByName { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        // customer sent a different amount than the order total
        public bool AmountMismatch => Amount != OrderTotal;

        public bool CanVerify => Status != "paid";

        public bool CanReject => Status != "failed";
    }
}