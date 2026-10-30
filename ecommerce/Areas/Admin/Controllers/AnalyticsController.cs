using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Enums;
using Pearl_Poetry.ViewModels.Admin.Analytics;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        // Bangladesh time (UTC+6, no daylight saving). Dates are stored in UTC.
        private static readonly TimeSpan LocalOffset = TimeSpan.FromHours(6);

        // these orders are not counted as sales
        private static readonly string[] NotRevenue = { "cancelled", "refunded", "returned" };

        private readonly EcommerceSystemContext _context;

        public AnalyticsController(EcommerceSystemContext context)
        {
            _context = context;
        }

        // GET: /Admin/Analytics?range=7d|30d|90d|12m
        [HttpGet]
        public async Task<IActionResult> Index(string? range)
        {
            range = range is "7d" or "90d" or "12m" ? range : "30d";

            // ---------- period (local dates) ----------
            DateTime todayLocal = (DateTime.UtcNow + LocalOffset).Date;
            DateTime endLocal = todayLocal.AddDays(1);
            bool monthly = range == "12m";

            DateTime startLocal = range switch
            {
                "7d" => todayLocal.AddDays(-6),
                "90d" => todayLocal.AddDays(-89),
                "12m" => new DateTime(todayLocal.Year, todayLocal.Month, 1).AddMonths(-11),
                _ => todayLocal.AddDays(-29)
            };

            // previous period of the same length (for the % change)
            DateTime prevStartLocal = startLocal - (endLocal - startLocal);

            DateTime startUtc = startLocal - LocalOffset;
            DateTime endUtc = endLocal - LocalOffset;
            DateTime prevStartUtc = prevStartLocal - LocalOffset;

            // ---------- orders of both periods (one query) ----------
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= prevStartUtc && o.CreatedAt < endUtc)
                .Select(o => new OrderRow
                {
                    Id = o.Id,
                    CreatedAtUtc = o.CreatedAt!.Value,
                    Total = o.TotalAmount,
                    Status = o.Status,
                    UserId = o.UserId,
                    UserName = o.User != null ? o.User.Name : null,
                    Method = o.Payments.OrderBy(p => p.Id).Select(p => p.PaymentMethod).FirstOrDefault() ?? "cod"
                })
                .ToListAsync();

            var current = orders.Where(o => o.CreatedAtUtc >= startUtc).ToList();
            var previous = orders.Where(o => o.CreatedAtUtc < startUtc).ToList();

            bool IsSale(OrderRow o) => !NotRevenue.Contains(o.Status);

            var currentSales = current.Where(IsSale).ToList();
            var previousSales = previous.Where(IsSale).ToList();

            // ---------- order items (products / categories / items sold) ----------
            var items = await _context.OrderItems
                .AsNoTracking()
                .Where(i => i.Order.CreatedAt >= prevStartUtc && i.Order.CreatedAt < endUtc &&
                            !NotRevenue.Contains(i.Order.Status))
                .Select(i => new
                {
                    i.ProductId,
                    i.ProductTitle,
                    i.Quantity,
                    i.Price,
                    IsCurrent = i.Order.CreatedAt >= startUtc,
                    CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : null
                })
                .ToListAsync();

            var currentItems = items.Where(i => i.IsCurrent).ToList();

            // ---------- new customers ----------
            int newCustomers = await _context.Users.CountAsync(u =>
                u.Role == UserRole.Customer && u.DeletedAt == null &&
                u.CreatedAt >= startUtc && u.CreatedAt < endUtc);

            int prevNewCustomers = await _context.Users.CountAsync(u =>
                u.Role == UserRole.Customer && u.DeletedAt == null &&
                u.CreatedAt >= prevStartUtc && u.CreatedAt < startUtc);

            // ---------- KPIs ----------
            decimal revenue = currentSales.Sum(o => o.Total);
            decimal prevRevenue = previousSales.Sum(o => o.Total);

            var model = new AnalyticsViewModel
            {
                Range = range,
                RangeLabel = range switch
                {
                    "7d" => "Last 7 days",
                    "90d" => "Last 90 days",
                    "12m" => "Last 12 months",
                    _ => "Last 30 days"
                },
                From = startLocal,
                To = todayLocal,
                Monthly = monthly,
                HasData = current.Count > 0,

                Revenue = new AnalyticsKpi { Value = revenue, Previous = prevRevenue },

                Orders = new AnalyticsKpi { Value = current.Count, Previous = previous.Count },

                AverageOrder = new AnalyticsKpi
                {
                    Value = currentSales.Count == 0 ? 0 : Math.Round(revenue / currentSales.Count, 2),
                    Previous = previousSales.Count == 0 ? 0 : Math.Round(prevRevenue / previousSales.Count, 2)
                },

                ItemsSold = new AnalyticsKpi
                {
                    Value = currentItems.Sum(i => i.Quantity),
                    Previous = items.Where(i => !i.IsCurrent).Sum(i => i.Quantity)
                },

                NewCustomers = new AnalyticsKpi { Value = newCustomers, Previous = prevNewCustomers },

                Cancelled = new AnalyticsKpi
                {
                    Value = current.Count(o => o.Status == "cancelled"),
                    Previous = previous.Count(o => o.Status == "cancelled"),
                    LowerIsBetter = true
                }
            };

            // ---------- revenue over time ----------
            DateTime Local(OrderRow o) => o.CreatedAtUtc + LocalOffset;

            if (monthly)
            {
                for (int m = 0; m < 12; m++)
                {
                    var s = startLocal.AddMonths(m);
                    var e = s.AddMonths(1);
                    var inBucket = current.Where(o => Local(o) >= s && Local(o) < e).ToList();

                    model.Series.Add(new AnalyticsPoint
                    {
                        Label = s.ToString("MMM yy"),
                        Revenue = inBucket.Where(IsSale).Sum(o => o.Total),
                        Orders = inBucket.Count
                    });
                }
            }
            else
            {
                for (var day = startLocal; day < endLocal; day = day.AddDays(1))
                {
                    var next = day.AddDays(1);
                    var inBucket = current.Where(o => Local(o) >= day && Local(o) < next).ToList();

                    model.Series.Add(new AnalyticsPoint
                    {
                        Label = day.ToString("dd MMM"),
                        Revenue = inBucket.Where(IsSale).Sum(o => o.Total),
                        Orders = inBucket.Count
                    });
                }
            }

            // ---------- orders by status (fixed order) ----------
            model.StatusBreakdown = Enum.GetNames<OrderStatus>()
                .Select(s => s.ToLower())
                .Select(s => new AnalyticsBar
                {
                    Label = s,
                    Count = current.Count(o => o.Status == s),
                    Amount = current.Where(o => o.Status == s).Sum(o => o.Total)
                })
                .Where(b => b.Count > 0)
                .ToList();

            // ---------- payment methods ----------
            model.PaymentMethods = currentSales
                .GroupBy(o => o.Method.ToLower())
                .Select(g => new AnalyticsBar
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(o => o.Total)
                })
                .OrderByDescending(b => b.Amount)
                .ToList();

            // ---------- top products ----------
            model.TopProducts = currentItems
                .GroupBy(i => new { i.ProductId, i.ProductTitle })
                .Select(g => new AnalyticsTopProduct
                {
                    ProductId = g.Key.ProductId,
                    Title = g.Key.ProductTitle,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(8)
                .ToList();

            // ---------- sales by category ----------
            model.TopCategories = currentItems
                .GroupBy(i => i.CategoryName ?? "Uncategorized")
                .Select(g => new AnalyticsBar
                {
                    Label = g.Key,
                    Count = g.Sum(i => i.Quantity),
                    Amount = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(b => b.Amount)
                .Take(8)
                .ToList();

            // ---------- top customers (registered only) ----------
            model.TopCustomers = currentSales
                .Where(o => o.UserId.HasValue)
                .GroupBy(o => new { UserId = o.UserId!.Value, o.UserName })
                .Select(g => new AnalyticsTopCustomer
                {
                    UserId = g.Key.UserId,
                    Name = g.Key.UserName ?? "Customer",
                    Orders = g.Count(),
                    Spent = g.Sum(o => o.Total)
                })
                .OrderByDescending(c => c.Spent)
                .Take(5)
                .ToList();

            return View(model);
        }

        private sealed class OrderRow
        {
            public int Id { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public decimal Total { get; set; }
            public string Status { get; set; } = string.Empty;
            public int? UserId { get; set; }
            public string? UserName { get; set; }
            public string Method { get; set; } = "cod";
        }
    }
}