using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Shop;

namespace Pearl_Poetry.Services
{
    public class WishlistService : IWishlistService
    {
        private const string FallbackImage = "/images/categories/fashion.jpg";

        private readonly EcommerceSystemContext _context;
        private readonly IShopperResolver _shopper;

        public WishlistService(EcommerceSystemContext context, IShopperResolver shopper)
        {
            _context = context;
            _shopper = shopper;
        }

        public async Task<WishlistViewModel> GetWishlistAsync()
        {
            var vm = new WishlistViewModel();

            var shopper = await _shopper.GetAsync();
            if (!shopper.UserId.HasValue) return vm;

            int userId = shopper.UserId.Value;

            // newest first
            var orderedIds = await _context.Wishlists
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Id)
                .Select(w => w.ProductId)
                .ToListAsync();

            if (orderedIds.Count == 0) return vm;

            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants)
                .Where(p => orderedIds.Contains(p.Id) && p.IsActive == true && p.DeletedAt == null)
                .ToListAsync();

            foreach (var id in orderedIds)
            {
                var p = products.FirstOrDefault(x => x.Id == id);
                if (p == null) continue;

                var variants = p.ProductVariants.Where(v => v.IsActive == true).ToList();
                bool hasVariants = variants.Count > 0;

                decimal price;
                decimal selling;
                bool inStock;

                if (hasVariants)
                {
                    var inStockVariants = variants.Where(v => (v.Quantity ?? 0) > 0).ToList();
                    inStock = inStockVariants.Count > 0;

                    // "from" price = cheapest variant that can really be bought
                    var best = (inStock ? inStockVariants : variants)
                        .Select(v =>
                        {
                            var (pr, d) = VariantHelper.ResolvePrice(p, v);
                            return new { Price = pr, Selling = PriceCalculator.GetSellingPrice(pr, d) };
                        })
                        .OrderBy(x => x.Selling)
                        .First();

                    price = best.Price;
                    selling = best.Selling;
                }
                else
                {
                    var (pr, d) = VariantHelper.ResolvePrice(p, null);
                    price = pr;
                    selling = PriceCalculator.GetSellingPrice(pr, d);
                    inStock = (p.Quantity ?? 0) > 0;
                }

                vm.Items.Add(new WishlistItemViewModel
                {
                    ProductId = p.Id,
                    Title = p.Title,
                    ImagePath = p.CoverImagePath ?? FallbackImage,
                    Price = selling,
                    OriginalPrice = selling < price ? price : null,
                    HasVariants = hasVariants,
                    InStock = inStock
                });
            }

            return vm;
        }

        public async Task<int> GetCountAsync()
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.UserId.HasValue) return 0;

            int userId = shopper.UserId.Value;
            return await _context.Wishlists.CountAsync(w => w.UserId == userId);
        }

        public async Task<HashSet<int>> GetProductIdsAsync()
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.UserId.HasValue) return new HashSet<int>();

            int userId = shopper.UserId.Value;
            var ids = await _context.Wishlists
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<WishlistToggleResult> ToggleAsync(int productId)
        {
            var shopper = await _shopper.GetAsync();

            // only registered customers have a wishlist (wishlist.user_id is NOT NULL)
            if (!shopper.UserId.HasValue)
            {
                return new WishlistToggleResult
                {
                    Success = false,
                    RequiresLogin = true,
                    Message = "Please log in to use your wishlist."
                };
            }

            int userId = shopper.UserId.Value;

            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            bool active;
            string message;

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                active = false;
                message = "Removed from your wishlist.";
            }
            else
            {
                bool productOk = await _context.Products
                    .AnyAsync(p => p.Id == productId && p.IsActive == true && p.DeletedAt == null);

                if (!productOk)
                {
                    return new WishlistToggleResult
                    {
                        Success = false,
                        Message = "This product is not available.",
                        Count = await _context.Wishlists.CountAsync(w => w.UserId == userId)
                    };
                }

                _context.Wishlists.Add(new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });
                active = true;
                message = "Added to your wishlist.";
            }

            await _context.SaveChangesAsync();

            return new WishlistToggleResult
            {
                Success = true,
                Active = active,
                Message = message,
                Count = await _context.Wishlists.CountAsync(w => w.UserId == userId)
            };
        }
    }
}