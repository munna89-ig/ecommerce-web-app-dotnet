
using Pearl_Poetry.Models.ViewModels.Admin;
using System;
using System.Collections.Generic;

namespace Pearl_Poetry.ViewModels.Admin.Dashboard
{
    public class DashboardViewModel
    {
        // ==========================================
        // ADMIN
        // ==========================================

        public string AdminName { get; set; } = "Admin";


        // ==========================================
        // MAIN STATISTICS
        // ==========================================

        public int TotalUsers { get; set; }

        public int NewUsersThisMonth { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalProducts { get; set; }

        public int OutOfStockProducts { get; set; }


        // ==========================================
        // QUICK STATISTICS
        // ==========================================

        public int ActiveProducts { get; set; }

        public int LowStockProducts { get; set; }

        public int UnpaidPayments { get; set; }


        // ==========================================
        // SALES CHART
        // ==========================================

        public List<SalesChartItemViewModel> SalesChart { get; set; }
            = new List<SalesChartItemViewModel>();


        // ==========================================
        // RECENT ORDERS
        // ==========================================

        public List<RecentOrderViewModel> RecentOrders { get; set; }
            = new List<RecentOrderViewModel>();


        // ==========================================
        // RECENT ACTIVITY
        // ==========================================

        public List<RecentActivityViewModel> RecentActivities { get; set; }
            = new List<RecentActivityViewModel>();
    }
}

