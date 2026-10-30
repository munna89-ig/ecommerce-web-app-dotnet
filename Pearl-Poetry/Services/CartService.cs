using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Cart;

namespace Pearl_Poetry.Services
{
    public class CartService : ICartService
    {
        private const string FallbackImage = "/images/categories/fashion.jpg";

        private readonly EcommerceSystemContext _context;
        private readonly IShopperResolver _shopper;

        public CartService(EcommerceSystemContext context, IShopperResolver shopper)
        {
            _context = context;
            _shopper = shopper;
        }

        // =====================================================================
        //  READ
        // =====================================================================
        public async Task<CartViewModel> GetCartAsync()
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.HasIdentity) return new CartViewModel();

            await MergeGuestCartAsync(shopper);

            var items = await OwnedItems(shopper)
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Variant).ThenInclude(v => v!.ProductImages)
                .OrderBy(c => c.Id)
                .ToListAsync();

            return new CartViewModel
            {
                Items = items
                    .Select(c => BuildLine(c.Product, c.Variant, c.Quantity ?? 1, c.Id))
                    .ToList()
            };
        }

        public async Task<int> GetCountAsync()
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.HasIdentity) return 0;

            await MergeGuestCartAsync(shopper);
            return await CountAsync(shopper);
        }

        // =====================================================================
        //  ADD
        // =====================================================================
        public async Task<CartActionResult> AddAsync(int productId, int? variantId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null || product.IsActive != true || product.DeletedAt != null)
                return CartActionResult.Fail("This product is not available.");

            // variants are decided by the DATABASE, never by what the browser says
            var activeVariants = product.ProductVariants.Where(v => v.IsActive == true).ToList();
            ProductVariant? variant = null;

            if (activeVariants.Count > 0)
            {
                if (!variantId.HasValue)
                    return CartActionResult.Fail("Please choose the options first.");

                variant = activeVariants.FirstOrDefault(v => v.Id == variantId.Value);
                if (variant == null)
                    return CartActionResult.Fail("The selected option is not available.");
            }

            int stock = VariantHelper.ResolveStock(product, variant);
            if (stock <= 0)
                return CartActionResult.Fail("Sorry, this item is out of stock.");

            var shopper = await _shopper.GetAsync(createGuest: true);
            await MergeGuestCartAsync(shopper);

            int? vId = variant?.Id;
            var existing = await OwnedItems(shopper)
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.VariantId == vId);

            int inCart = existing?.Quantity ?? 0;
            string message = "Added to your cart.";

            if (inCart + quantity > stock)
            {
                int canAdd = stock - inCart;
                if (canAdd <= 0)
                    return CartActionResult.Fail($"You already have all {stock} available piece(s) in your cart.");

                quantity = canAdd;
                message = $"Only {stock} available - your cart now has the maximum.";
            }

            var now = DateTime.UtcNow;

            if (existing != null)
            {
                existing.Quantity = inCart + quantity;
                existing.UpdatedAt = now;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = shopper.UserId,
                    GuestUserId = shopper.UserId.HasValue ? null : shopper.GuestUserId,
                    ProductId = productId,
                    VariantId = vId,
                    Quantity = quantity,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            return new CartActionResult
            {
                Success = true,
                Message = message,
                Count = await CountAsync(shopper)
            };
        }

        // =====================================================================
        //  UPDATE
        // =====================================================================
        public async Task<CartActionResult> UpdateAsync(int itemId, int quantity)
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.HasIdentity) return CartActionResult.Fail("Your cart is empty.");

            var item = await OwnedItems(shopper)
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.Id == itemId);

            if (item == null) return CartActionResult.Fail("This item is no longer in your cart.");

            if (!VariantHelper.IsSellable(item.Product, item.Variant))
                return CartActionResult.Fail("This item is no longer available.");

            int stock = VariantHelper.ResolveStock(item.Product, item.Variant);
            if (stock <= 0) return CartActionResult.Fail("This item is out of stock.");

            if (quantity < 1) quantity = 1;

            string message = string.Empty;
            bool ok = true;

            if (quantity > stock)
            {
                quantity = stock;
                ok = false;
                message = $"Only {stock} available.";
            }

            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var cart = await GetCartAsync();
            var line = cart.Items.FirstOrDefault(i => i.ItemId == itemId);

            return new CartActionResult
            {
                Success = ok,
                Message = message,
                Quantity = quantity,
                LineTotal = line?.LineTotal ?? 0,
                Subtotal = cart.Subtotal,
                Count = cart.ItemCount
            };
        }

        // =====================================================================
        //  REMOVE
        // =====================================================================
        public async Task<CartActionResult> RemoveAsync(int itemId)
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.HasIdentity) return CartActionResult.Fail("Your cart is empty.");

            var item = await OwnedItems(shopper).FirstOrDefaultAsync(c => c.Id == itemId);
            if (item == null) return CartActionResult.Fail("This item is no longer in your cart.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            var cart = await GetCartAsync();

            return new CartActionResult
            {
                Success = true,
                Message = "Item removed.",
                Subtotal = cart.Subtotal,
                Count = cart.ItemCount
            };
        }

        // =====================================================================
        //  SHARED: one cart line (also used by the checkout page)
        // =====================================================================
        public static CartItemViewModel BuildLine(Product product, ProductVariant? variant, int quantity, int itemId)
        {
            var (price, discount) = VariantHelper.ResolvePrice(product, variant);
            decimal selling = PriceCalculator.GetSellingPrice(price, discount);
            int stock = VariantHelper.ResolveStock(product, variant);

            string? problem = null;
            if (!VariantHelper.IsSellable(product, variant)) problem = "This item is no longer available";
            else if (stock <= 0) problem = "Out of stock";
            else if (quantity > stock) problem = $"Only {stock} left in stock";

            string? variantImage = variant?.ProductImages
                .OrderBy(i => i.SortOrder ?? 0)
                .Select(i => i.ImagePath)
                .FirstOrDefault();

            return new CartItemViewModel
            {
                ItemId = itemId,
                ProductId = product.Id,
                VariantId = variant?.Id,
                Title = product.Title,
                ImagePath = variantImage ?? product.CoverImagePath ?? FallbackImage,
                // null when color AND size are empty => nothing is shown
                VariantLabel = variant == null ? null : VariantHelper.BuildLabel(variant.Color, variant.Size),
                UnitPrice = selling,
                OriginalPrice = selling < price ? price : null,
                Quantity = quantity,
                Stock = stock,
                Problem = problem
            };
        }

        // =====================================================================
        //  PRIVATE
        // =====================================================================
        private IQueryable<CartItem> OwnedItems(Shopper shopper)
        {
            if (shopper.UserId.HasValue)
            {
                int userId = shopper.UserId.Value;
                return _context.CartItems.Where(c => c.UserId == userId);
            }

            if (shopper.GuestUserId.HasValue)
            {
                int guestId = shopper.GuestUserId.Value;
                return _context.CartItems.Where(c => c.GuestUserId == guestId && c.UserId == null);
            }

            return _context.CartItems.Where(c => false);
        }

        private Task<int> CountAsync(Shopper shopper)
            => OwnedItems(shopper).SumAsync(c => c.Quantity ?? 0);

        // After login the guest cart (cookie) is moved into the user's cart.
        private async Task MergeGuestCartAsync(Shopper shopper)
        {
            if (!shopper.UserId.HasValue || !shopper.GuestUserId.HasValue) return;

            int userId = shopper.UserId.Value;
            int guestId = shopper.GuestUserId.Value;

            var guestItems = await _context.CartItems
                .Where(c => c.GuestUserId == guestId && c.UserId == null)
                .ToListAsync();

            if (guestItems.Count == 0) return;

            var userItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var g in guestItems)
            {
                var same = userItems.FirstOrDefault(u => u.ProductId == g.ProductId && u.VariantId == g.VariantId);

                if (same != null)
                {
                    same.Quantity = (same.Quantity ?? 0) + (g.Quantity ?? 0);
                    same.UpdatedAt = now;
                    _context.CartItems.Remove(g);
                }
                else
                {
                    g.UserId = userId;
                    g.GuestUserId = null;
                    g.UpdatedAt = now;
                    userItems.Add(g);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}