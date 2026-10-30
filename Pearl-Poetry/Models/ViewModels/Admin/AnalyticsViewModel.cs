namespace Pearl_Poetry.ViewModels.Admin.Analytics
{
    public class AnalyticsViewModel
    {
        // "7d" | "30d" | "90d" | "12m"
        public string Range { get; set; } = "30d";

        public string RangeLabel { get; set; } = string.Empty;

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public bool Monthly { get; set; }

        public bool HasData { get; set; }

        // ---------- KPI tiles ----------
        public AnalyticsKpi Revenue { get; set; } = new();

        public AnalyticsKpi Orders { get; set; } = new();

        public AnalyticsKpi AverageOrder { get; set; } = new();

        public AnalyticsKpi ItemsSold { get; set; } = new();

        public AnalyticsKpi NewCustomers { get; set; } = new();

        public AnalyticsKpi Cancelled { get; set; } = new() { LowerIsBetter = true };

        // ---------- charts / lists ----------
        public List<AnalyticsPoint> Series { get; set; } = new();

        public List<AnalyticsBar> StatusBreakdown { get; set; } = new();

        public List<AnalyticsBar> PaymentMethods { get; set; } = new();

        public List<AnalyticsBar> TopCategories { get; set; } = new();

        public List<AnalyticsTopProduct> TopProducts { get; set; } = new();

        public List<AnalyticsTopCustomer> TopCustomers { get; set; } = new();
    }

    public class AnalyticsKpi
    {
        public decimal Value { get; set; }

        public decimal Previous { get; set; }

        // e.g. cancelled orders: going down is good
        public bool LowerIsBetter { get; set; }

        // null when there is nothing to compare with
        public decimal? ChangePercent =>
            Previous == 0 ? null : Math.Round((Value - Previous) / Previous * 100m, 1);

        public bool IsGood => ChangePercent.HasValue &&
                              (LowerIsBetter ? ChangePercent < 0 : ChangePercent > 0);
    }

    public class AnalyticsPoint
    {
        public string Label { get; set; } = string.Empty;

        public decimal Revenue { get; set; }

        public int Orders { get; set; }
    }

    public class AnalyticsBar
    {
        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }

    public class AnalyticsTopProduct
    {
        public int? ProductId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Revenue { get; set; }
    }

    public class AnalyticsTopCustomer
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Orders { get; set; }

        public decimal Spent { get; set; }
    }
}