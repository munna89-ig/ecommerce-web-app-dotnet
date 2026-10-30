using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Models.ViewModels.Admin;
using Pearl_Poetry.ViewModels.Admin.Dashboard;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly EcommerceSystemContext _context;

        public DashboardController(EcommerceSystemContext context)
        {
            _context = context;
        }


        // ============================================================
        // DASHBOARD
        // GET: /Admin/Dashboard
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // --------------------------------------------------------
            // DATE INFORMATION
            // --------------------------------------------------------

            var now = DateTime.UtcNow;

            var today = now.Date;

            var startOfMonth = new DateTime(
                now.Year,
                now.Month,
                1
            );

            var startOfLast7Days = today.AddDays(-6);


            // ========================================================
            // ADMIN NAME
            // ========================================================

            var adminName = "Admin";

            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                var currentUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);

                if (currentUser != null &&
                    !string.IsNullOrWhiteSpace(currentUser.Name))
                {
                    adminName = currentUser.Name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(User.Identity?.Name))
            {
                adminName = User.Identity!.Name!;
            }


            // ========================================================
            // USER STATISTICS
            // ========================================================

            var totalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.DeletedAt == null);


            var newUsersThisMonth = await _context.Users
                .AsNoTracking()
                .CountAsync(u =>
                    u.DeletedAt == null &&
                    u.CreatedAt >= startOfMonth
                );


            // ========================================================
            // ORDER STATISTICS
            // ========================================================

            var totalOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync();


            var pendingOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o =>
                    o.Status == "pending"
                );


            // ========================================================
            // TOTAL REVENUE
            // ========================================================
            //
            // Same idea as your PHP dashboard:
            //
            // Query::table('orders')->sum('total_amount')
            //
            // We exclude cancelled/refunded orders because they
            // should not count as actual sales revenue.
            // ========================================================

            var totalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.Status != "cancelled" &&
                    o.Status != "refunded"
                )
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;


            // ========================================================
            // PRODUCT STATISTICS
            // ========================================================

            var totalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p =>
                    p.DeletedAt == null
                );


            var activeProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p =>
                    p.DeletedAt == null &&
                    (p.IsActive ?? false)
                );


            var outOfStockProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p =>
                    p.DeletedAt == null &&
                    (p.Quantity ?? 0) <= 0
                );


            // --------------------------------------------------------
            // LOW STOCK
            //
            // Currently using <= 5 as the low-stock threshold.
            // --------------------------------------------------------

            var lowStockProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p =>
                    p.DeletedAt == null &&
                    (p.Quantity ?? 0) > 0 &&
                    (p.Quantity ?? 0) <= 5
                );


            // ========================================================
            // UNPAID PAYMENTS
            // ========================================================

           

            var unpaidPayments = await _context.Payments
               .AsNoTracking()
               .CountAsync(p =>
                   p.Status == "pending_verification"
               );

            // ========================================================
            // SALES - LAST 7 DAYS
            // ========================================================

            var last7DaysOrders = await _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.CreatedAt >= startOfLast7Days &&
                    o.Status != "cancelled" &&
                    o.Status != "refunded"
                )
                .Select(o => new
                {
                    o.CreatedAt,
                    o.TotalAmount
                })
                .ToListAsync();


            var salesChart =
                Enumerable
                    .Range(0, 7)
                    .Select(i =>
                    {
                        var date = startOfLast7Days.AddDays(i);

                        var amount = last7DaysOrders
                            .Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date)
                            .Sum(o => o.TotalAmount);

                        return new SalesChartItemViewModel
                        {
                            Date = date,

                            DayName = date.ToString("ddd"),

                            Amount = amount
                        };
                    })
                    .ToList();


            // ========================================================
            // RECENT ORDERS
            // ========================================================

            var recentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    Id = o.Id,

                    OrderNumber = o.OrderNumber,

                    CustomerName =
                        o.User != null &&
                        !string.IsNullOrWhiteSpace(o.User.Name)
                            ? o.User.Name
                            : (
                                !string.IsNullOrWhiteSpace(o.ShipFullName)
                                    ? o.ShipFullName
                                    : "Guest"
                              ),

                    TotalAmount = o.TotalAmount,

                    Status = o.Status,

                    PaymentStatus = o.PaymentStatus,

                    CreatedAt = (DateTime)o.CreatedAt
                })
                .ToListAsync();


            // ========================================================
            // RECENT AUDIT ACTIVITY
            // ========================================================
            //
            // AuditLog does not contain a Description field.
            // It contains:
            //
            // Action
            // EntityType
            // EntityId
            // CreatedAt
            //
            // Therefore we create the display description here.
            // ========================================================

            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new
                {
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.CreatedAt
                })
                .ToListAsync();


            var recentActivities = auditLogs
                .Select(a =>
                {
                    var action =
                        string.IsNullOrWhiteSpace(a.Action)
                            ? "Activity"
                            : a.Action;

                    var entity =
                        string.IsNullOrWhiteSpace(a.EntityType)
                            ? "item"
                            : a.EntityType;

                    var description =
                        a.EntityId.HasValue
                            ? $"{action} {entity} #{a.EntityId.Value}"
                            : $"{action} {entity}";

                    return new RecentActivityViewModel
                    {
                        Action = action,

                        EntityType = entity,

                        EntityId = a.EntityId,

                        Description = description,

                        CreatedAt = (DateTime)a.CreatedAt
                    };
                })
                .ToList();


            // ========================================================
            // BUILD DASHBOARD MODEL
            // ========================================================

            var model = new DashboardViewModel
            {
                // Admin
                AdminName = adminName,

                // Users
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,

                // Orders
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,

                // Revenue
                TotalRevenue = totalRevenue,

                // Products
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                OutOfStockProducts = outOfStockProducts,
                LowStockProducts = lowStockProducts,

                // Payments
                UnpaidPayments = unpaidPayments,

                // Chart
                SalesChart = salesChart,

                // Recent data
                RecentOrders = recentOrders,
                RecentActivities = recentActivities
            };


            return View(model);
        }
    }
}

