namespace Pearl_Poetry.Models.ViewModels.Admin
{
    public class SalesChartItemViewModel
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty; 
        public decimal Amount { get; set; }
    }
}
