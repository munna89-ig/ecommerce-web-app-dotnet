using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Shop;

namespace Pearl_Poetry.Controllers
{
    public class ProductController : Controller
    {
        private const string FallbackImage = "/images/categories/fashion.jpg";
        private const int SearchPageSize = 15;

        private readonly EcommerceSystemContext _context;
        private readonly IWishlistService _wishlist;

        public ProductController(EcommerceSystemContext context, IWishlistService wishlist)
        {
            _context = context;
            _wishlist = wishlist;
        }

        // =========================================================
        // SEARCH / CATEGORY
        // /Product/Search?q=ring
        // /Product/Search?category=jewelry
        // /Product/Search?q=gold&category=jewelry&sort=price_asc&page=2
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Search(string? q, string? category, string? sort, int page = 1)
        {
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            if (q != null && q.Length > 100) q = q[..100];

            category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            sort = sort is "price_asc" or "price_desc" or "name" ? sort : "newest";
            if (page < 1) page = 1;

            // all active categories (small table => kept in memory)
            var allCategories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true)
                .Select(c => new { c.Id, c.ParentId, c.Name, c.Slug })
                .ToListAsync();

            // category + all of its sub-categories
            List<int> WithChildren(IEnumerable<int> roots)
            {
                var result = new HashSet<int>(roots);
                bool added = true;

                while (added)
                {
                    added = false;
                    foreach (var c in allCategories)
                    {
                        if (c.ParentId.HasValue && result.Contains(c.ParentId.Value) && result.Add(c.Id))
                            added = true;
                    }
                }

                return result.ToList();
            }

            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true && p.DeletedAt == null);

            // ---------- 1. category filter ----------
            var current = category == null
                ? null
                : allCategories.FirstOrDefault(c => string.Equals(c.Slug, category, StringComparison.OrdinalIgnoreCase));

            if (current != null)
            {
                var ids = WithChildren(new[] { current.Id });

                products = products.Where(p =>
                    (p.CategoryId != null && ids.Contains(p.CategoryId.Value)) ||
                    p.Categories.Any(c => ids.Contains(c.Id)));
            }

            // ---------- 2. text search ----------
            // every word must match the product text OR the product's category name
            if (q != null)
            {
                var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5).ToList();

                foreach (var term in terms)
                {
                    string t = term;

                    var termCatIds = WithChildren(allCategories
                        .Where(c => c.Name.Contains(t, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Id));

                    products = products.Where(p =>
                        p.Title.Contains(t) ||
                        (p.Sku != null && p.Sku.Contains(t)) ||
                        (p.ShortDescription != null && p.ShortDescription.Contains(t)) ||
                        p.ProductType.Contains(t) ||
                        (p.CategoryId != null && termCatIds.Contains(p.CategoryId.Value)) ||
                        p.Categories.Any(c => termCatIds.Contains(c.Id)));
                }
            }

            // ---------- 3. sort ----------
            products = sort switch
            {
                "price_asc" => products
                    .OrderBy(p => p.DiscountPrice != null && p.DiscountPrice > 0 && p.DiscountPrice < p.Price
                        ? p.DiscountPrice.Value : p.Price)
                    .ThenByDescending(p => p.Id),

                "price_desc" => products
                    .OrderByDescending(p => p.DiscountPrice != null && p.DiscountPrice > 0 && p.DiscountPrice < p.Price
                        ? p.DiscountPrice.Value : p.Price)
                    .ThenByDescending(p => p.Id),

                "name" => products.OrderBy(p => p.Title),

                _ => products.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            };

            // ---------- 4. paging ----------
            int totalItems = await products.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)SearchPageSize));
            if (page > totalPages) page = totalPages;

            var items = await products
                .Skip((page - 1) * SearchPageSize)
                .Take(SearchPageSize)
                .Select(p => new SearchProductItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Sku = p.Sku,
                    CoverImage = p.CoverImagePath ?? FallbackImage,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    HasVariants = p.ProductVariants.Any(v => v.IsActive == true)
                })
                .ToListAsync();

            var wishIds = await _wishlist.GetProductIdsAsync();
            foreach (var item in items)
            {
                item.InWishlist = wishIds.Contains(item.Id);
                if (string.IsNullOrWhiteSpace(item.CoverImage)) item.CoverImage = FallbackImage;
            }

            // ---------- 5. sidebar counts ----------
            var directCounts = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true && p.DeletedAt == null && p.CategoryId != null)
                .GroupBy(p => p.CategoryId!.Value)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            int allCount = await _context.Products
                .CountAsync(p => p.IsActive == true && p.DeletedAt == null);

            var sidebar = allCategories
                .OrderBy(c => c.Name)
                .Select(c => new SearchCategoryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Count = WithChildren(new[] { c.Id }).Sum(id => directCounts.GetValueOrDefault(id))
                })
                .ToList();

            var model = new ProductSearchViewModel
            {
                Query = q,
                CategorySlug = current?.Slug,
                CategoryName = current?.Name,
                Sort = sort,
                Page = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                AllCount = allCount,
                Categories = sidebar,
                Products = items,
                MatchedCategories = q == null
                    ? new List<SearchCategoryItem>()
                    : sidebar.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList()
            };

            return View(model);
        }

        // =========================================================
        // DETAILS  /Product/Details/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive == true && p.DeletedAt == null);

            if (product == null) return NotFound();

            // ---- gallery: cover first, then the other images (no duplicates) ----
            var images = new List<string>();
            if (!string.IsNullOrWhiteSpace(product.CoverImagePath)) images.Add(product.CoverImagePath!);

            foreach (var img in product.ProductImages.OrderBy(i => i.SortOrder ?? 0).ThenBy(i => i.Id))
            {
                if (!images.Any(x => string.Equals(x, img.ImagePath, StringComparison.OrdinalIgnoreCase)))
                    images.Add(img.ImagePath);
            }

            if (images.Count == 0) images.Add(FallbackImage);

            // ---- variants: empty color / size become null so the page hides them ----
            var variants = product.ProductVariants
                .Where(v => v.IsActive == true)
                .OrderBy(v => v.Id)
                .Select(v =>
                {
                    var (price, discount) = VariantHelper.ResolvePrice(product, v);

                    return new ShopVariantViewModel
                    {
                        Id = v.Id,
                        Color = VariantHelper.Clean(v.Color),
                        Size = VariantHelper.Clean(v.Size),
                        Price = price,
                        DiscountPrice = discount,
                        Stock = v.Quantity ?? 0
                    };
                })
                .ToList();

            var wishIds = await _wishlist.GetProductIdsAsync();

            var model = new ShopProductDetailsViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Sku = product.Sku,
                CategoryName = product.Category?.Name,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Quantity ?? 0,
                CoverImage = images[0],
                Images = images,
                Variants = variants,
                InWishlist = wishIds.Contains(product.Id)
            };

            return View(model);
        }
    }
}