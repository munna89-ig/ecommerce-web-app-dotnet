using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Services.Interfaces;

namespace Pearl_Poetry.Controllers
{
    public class HomeController(EcommerceSystemContext _context, IWishlistService _wishlist) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // ---------- hero banners (Admin > Banners) ----------
            var today = DateTime.UtcNow.AddHours(6).Date;   // Bangladesh date

            ViewBag.Banners = await _context.Banners
                .AsNoTracking()
                .Where(b => b.IsActive == true &&
                            b.Position == "homepage_hero" &&
                            (b.StartDate == null || b.StartDate <= today) &&
                            (b.EndDate == null || b.EndDate >= today))
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive == true)
                .ToListAsync();

            ViewBag.NewArrivals = await _context.Products
                .Where(p => p.IsActive == true && p.DeletedAt == null)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            ViewBag.AllItems = await _context.Products
                .Where(p => p.IsActive == true && p.DeletedAt == null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // products that need a color / size choice => the cart button opens the product page
            ViewBag.VariantProductIds = (await _context.ProductVariants
                .Where(v => v.IsActive == true)
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync()).ToHashSet();

            // hearts that must be shown as "active"
            ViewBag.WishlistIds = await _wishlist.GetProductIdsAsync();

            ViewBag.Settings = await _context.Settings.FirstOrDefaultAsync();

            return View();
        }
    }
}