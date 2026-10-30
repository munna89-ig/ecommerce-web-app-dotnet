namespace Pearl_Poetry.Models.ViewModels.Admin
{
    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty; public string CustomerName { get; set; } = "Guest"; 
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "pending"; public string PaymentStatus { get; set; } = "unpaid"; 
        public DateTime CreatedAt { get; set; }
    }
}
