using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Cart;
using Pearl_Poetry.ViewModels.Checkout;

namespace Pearl_Poetry.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly EcommerceSystemContext _context;

        private readonly IShopperResolver _shopper;
        private readonly ICartService _cart;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            EcommerceSystemContext context,
            IShopperResolver shopper,
            ICartService cart,
            ILogger<CheckoutService> logger)
        {
            _context = context;
            _shopper = shopper;
            _cart = cart;
            _logger = logger;
        }

        // =====================================================================
        //  CHECKOUT PAGE
        // =====================================================================
        public async Task<CheckoutViewModel?> StartAsync(int? productId, int? variantId, int quantity)
        {
            var model = new CheckoutViewModel();

            if (productId.HasValue)
            {
                model.BuyNowProductId = productId;
                model.BuyNowVariantId = variantId;
                model.BuyNowQuantity = Math.Max(quantity, 1);
            }

            if (!await LoadDisplayAsync(model)) return null;

            await PrefillAsync(model);
            model.DeliveryCharge = GetDeliveryCharge(model, model.District);
            model.Total = PriceCalculator.CalculateTotal(model.Subtotal, 0, model.DeliveryCharge);

            return model;
        }

        public async Task<bool> LoadDisplayAsync(CheckoutViewModel model)
        {
            model.Items = model.IsBuyNow
                ? await BuildBuyNowItemsAsync(model.BuyNowProductId!.Value, model.BuyNowVariantId, model.BuyNowQuantity)
                : (await _cart.GetCartAsync()).Items;

            if (model.Items.Count == 0) return false;

            var settings = await _context.Settings.AsNoTracking().FirstOrDefaultAsync();

            model.InsideDhakaCharge = settings?.InsideDhakaCharge ?? settings?.ShippingFee ?? 0;
            model.OutsideDhakaCharge = settings?.OutsideDhakaCharge ?? settings?.ShippingFee ?? 0;
            model.BkashNumber = settings?.BkashNumber;
            model.BkashQrImage = NormalizeQrPath(settings?.BkashQrImage);

            model.Subtotal = model.Items.Where(i => i.IsAvailable).Sum(i => i.LineTotal);
            model.DeliveryCharge = GetDeliveryCharge(model, model.District);
            model.Total = PriceCalculator.CalculateTotal(model.Subtotal, 0, model.DeliveryCharge);

            return true;
        }

        // =====================================================================
        //  PLACE ORDER
        // =====================================================================
        public async Task<PlaceOrderResult> PlaceOrderAsync(CheckoutViewModel model)
        {
            var shopper = await _shopper.GetAsync(createGuest: true);

            // ---- 1. what is being bought (re-read from the DB, never trust the browser) ----
            var lines = new List<(int ProductId, int? VariantId, int Quantity)>();
            List<int> cartItemIds = new();

            if (model.IsBuyNow)
            {
                lines.Add((model.BuyNowProductId!.Value, model.BuyNowVariantId, Math.Max(model.BuyNowQuantity, 1)));
            }
            else
            {
                var cart = await _cart.GetCartAsync();
                foreach (var i in cart.Items)
                {
                    lines.Add((i.ProductId, i.VariantId, i.Quantity));
                    cartItemIds.Add(i.ItemId);
                }
            }

            if (lines.Count == 0) return PlaceOrderResult.Fail("Your cart is empty.");

            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // ---- 2. validate every line + build order items ----
            var now = DateTime.UtcNow;
            var orderItems = new List<OrderItem>();
            var stockChanges = new List<(int ProductId, int? VariantId, int Quantity)>();
            decimal subtotal = 0;

            foreach (var l in lines)
            {
                var product = products.FirstOrDefault(p => p.Id == l.ProductId);
                if (product == null)
                    return PlaceOrderResult.Fail("An item in your order no longer exists.");

                var activeVariants = product.ProductVariants.Where(v => v.IsActive == true).ToList();
                ProductVariant? variant = null;

                if (activeVariants.Count > 0)
                {
                    variant = activeVariants.FirstOrDefault(v => v.Id == l.VariantId);
                    if (variant == null)
                        return PlaceOrderResult.Fail($"Please choose a valid option for \"{product.Title}\".");
                }

                if (!VariantHelper.IsSellable(product, variant))
                    return PlaceOrderResult.Fail($"\"{product.Title}\" is no longer available.");

                int stock = VariantHelper.ResolveStock(product, variant);
                if (l.Quantity > stock)
                {
                    return PlaceOrderResult.Fail(stock <= 0
                        ? $"\"{product.Title}\" is out of stock."
                        : $"Only {stock} piece(s) of \"{product.Title}\" are left.");
                }

                var (price, discount) = VariantHelper.ResolvePrice(product, variant);
                decimal selling = PriceCalculator.GetSellingPrice(price, discount);
                subtotal += selling * l.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    VariantId = variant?.Id,
                    ProductTitle = product.Title,
                    // null when the variant has neither color nor size
                    VariantLabel = variant == null ? null : VariantHelper.BuildLabel(variant.Color, variant.Size),
                    Quantity = l.Quantity,
                    Price = selling,
                    CreatedAt = now
                });

                stockChanges.Add((product.Id, variant?.Id, l.Quantity));
            }

            // ---- 3. money ----
            var settings = await _context.Settings.AsNoTracking().FirstOrDefaultAsync();
            decimal delivery = GetDeliveryCharge(settings, model.District);
            decimal total = PriceCalculator.CalculateTotal(subtotal, 0, delivery);

            bool isBkash = string.Equals(model.PaymentMethod, "bkash", StringComparison.OrdinalIgnoreCase);

            // ---- 4. save everything in ONE transaction ----
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                DateTime? nowN = now;

                // atomic stock deduction: the WHERE guarantees we never go below zero
                foreach (var s in stockChanges)
                {
                    int qty = s.Quantity;
                    int affected;

                    if (s.VariantId.HasValue)
                    {
                        int vid = s.VariantId.Value;
                        affected = await _context.ProductVariants
                            .Where(v => v.Id == vid && v.Quantity >= qty)
                            .ExecuteUpdateAsync(u => u
                                .SetProperty(v => v.Quantity, v => v.Quantity - qty)
                                .SetProperty(v => v.UpdatedAt, nowN));
                    }
                    else
                    {
                        int pid = s.ProductId;
                        affected = await _context.Products
                            .Where(p => p.Id == pid && p.Quantity >= qty)
                            .ExecuteUpdateAsync(u => u
                                .SetProperty(p => p.Quantity, p => p.Quantity - qty)
                                .SetProperty(p => p.UpdatedAt, nowN));
                    }

                    if (affected == 0)
                    {
                        await tx.RollbackAsync();
                        return PlaceOrderResult.Fail("Sorry, an item in your order just went out of stock. Please review your order.");
                    }
                }

                int? userId = shopper.UserId;
                int? guestId = userId.HasValue ? null : shopper.GuestUserId;

                var address = new ShippingAddress
                {
                    UserId = userId,
                    GuestUserId = guestId,
                    FullName = model.FullName.Trim(),
                    Email = Clean(model.Email),
                    Phone = model.Phone.Trim(),
                    District = model.District,
                    Address = model.Address.Trim(),
                    PostalCode = Clean(model.PostalCode),
                    Country = "Bangladesh",
                    DeliveryNote = Clean(model.DeliveryNote),
                    IsDefault = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var order = new Order
                {
                    OrderNumber = await UniqueOrderNumberAsync(),
                    UserId = userId,
                    GuestUserId = guestId,
                    ShippingAddress = address,
                    ShipFullName = address.FullName!,
                    ShipEmail = address.Email,
                    ShipPhone = address.Phone!,
                    ShipDistrict = address.District,
                    ShipAddress = address.Address,
                    ShipPostalCode = address.PostalCode,
                    ShipCountry = "Bangladesh",
                    ShipDeliveryNote = address.DeliveryNote,
                    Subtotal = subtotal,
                    DiscountAmount = 0,
                    DeliveryCharge = delivery,
                    TotalAmount = total,
                    Status = "pending",
                    PaymentStatus = isBkash ? "pending_verification" : "unpaid",
                    CreatedAt = now,
                    UpdatedAt = now
                };

                foreach (var item in orderItems) order.OrderItems.Add(item);

                order.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    Status = "pending",
                    Note = "Order placed by customer",
                    CreatedAt = now
                });

                if (isBkash)
                {
                    order.Payments.Add(new Payment
                    {
                        UserId = userId,
                        GuestUserId = guestId,
                        PaymentMethod = "bkash",
                        TransactionId = Clean(model.TransactionId),
                        SenderName = Clean(model.SenderName),
                        SenderPhone = Clean(model.SenderPhone),
                        Amount = total,
                        Status = "pending_verification",
                        CreatedAt = now
                    });
                }

                _context.Orders.Add(order);

                // remember the contact details of a guest
                if (guestId.HasValue)
                {
                    var guest = await _context.GuestUsers.FirstOrDefaultAsync(g => g.Id == guestId.Value);
                    if (guest != null)
                    {
                        guest.Name = address.FullName;
                        guest.Email = address.Email;
                        guest.Phone = address.Phone;
                    }
                }

                // "Buy now" does not touch the cart; a normal checkout empties it
                if (cartItemIds.Count > 0)
                {
                    var cartRows = await _context.CartItems
                        .Where(c => cartItemIds.Contains(c.Id))
                        .ToListAsync();
                    _context.CartItems.RemoveRange(cartRows);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return PlaceOrderResult.Ok(order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Placing the order failed.");
                await tx.RollbackAsync();
                return PlaceOrderResult.Fail("Something went wrong while placing your order. Please try again.");
            }
        }

        // =====================================================================
        //  SUCCESS PAGE
        // =====================================================================
        public async Task<OrderSuccessViewModel?> GetSuccessAsync(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber)) return null;

            var shopper = await _shopper.GetAsync();
            if (!shopper.HasIdentity) return null;

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return null;

            // an order can only be seen by the person who placed it
            bool mine =
                (shopper.UserId.HasValue && order.UserId == shopper.UserId) ||
                (shopper.GuestUserId.HasValue && order.GuestUserId == shopper.GuestUserId);

            if (!mine) return null;

            var payment = order.Payments.OrderBy(p => p.Id).FirstOrDefault();
            bool isBkash = string.Equals(payment?.PaymentMethod, "bkash", StringComparison.OrdinalIgnoreCase);

            var addressParts = new List<string> { order.ShipAddress };
            if (!string.IsNullOrWhiteSpace(order.ShipDistrict)) addressParts.Add(order.ShipDistrict!);
            if (!string.IsNullOrWhiteSpace(order.ShipPostalCode)) addressParts.Add(order.ShipPostalCode!);

            return new OrderSuccessViewModel
            {
                OrderNumber = order.OrderNumber,
                FullName = order.ShipFullName,
                Phone = order.ShipPhone,
                FullAddress = string.Join(", ", addressParts),
                PaymentMethodLabel = isBkash ? "bKash" : "Cash on Delivery",
                PaymentNeedsVerification = isBkash && order.PaymentStatus == "pending_verification",
                Subtotal = order.Subtotal ?? 0,
                DeliveryCharge = order.DeliveryCharge ?? 0,
                Total = order.TotalAmount,
                Items = order.OrderItems
                    .OrderBy(i => i.Id)
                    .Select(i => new OrderSuccessItem
                    {
                        Title = i.ProductTitle,
                        VariantLabel = i.VariantLabel,
                        Quantity = i.Quantity,
                        Price = i.Price
                    })
                    .ToList()
            };
        }

        // =====================================================================
        //  PRIVATE
        // =====================================================================
        private async Task<List<CartItemViewModel>> BuildBuyNowItemsAsync(int productId, int? variantId, int quantity)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants).ThenInclude(v => v.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return new List<CartItemViewModel>();

            var activeVariants = product.ProductVariants.Where(v => v.IsActive == true).ToList();
            ProductVariant? variant = null;

            if (activeVariants.Count > 0)
            {
                variant = activeVariants.FirstOrDefault(v => v.Id == variantId);
                if (variant == null) return new List<CartItemViewModel>();
            }

            return new List<CartItemViewModel>
            {
                CartService.BuildLine(product, variant, Math.Max(quantity, 1), 0)
            };
        }

        private async Task PrefillAsync(CheckoutViewModel model)
        {
            var shopper = await _shopper.GetAsync();
            if (!shopper.UserId.HasValue) return;

            int userId = shopper.UserId.Value;

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var last = await _context.ShippingAddresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            model.FullName = last?.FullName ?? user?.Name ?? string.Empty;
            model.Email = last?.Email ?? user?.Email;
            model.Phone = last?.Phone ?? user?.Phone ?? string.Empty;

            if (last != null)
            {
                if (!string.IsNullOrWhiteSpace(last.District)) model.District = last.District!;
                model.Address = last.Address;
                model.PostalCode = last.PostalCode;
            }
        }

        private static decimal GetDeliveryCharge(CheckoutViewModel model, string? district)
        {
            bool inside = string.Equals(district?.Trim(), "Dhaka", StringComparison.OrdinalIgnoreCase);
            return inside ? model.InsideDhakaCharge : model.OutsideDhakaCharge;
        }

        private static decimal GetDeliveryCharge(Setting? settings, string? district)
        {
            bool inside = string.Equals(district?.Trim(), "Dhaka", StringComparison.OrdinalIgnoreCase);

            return inside
                ? settings?.InsideDhakaCharge ?? settings?.ShippingFee ?? 0
                : settings?.OutsideDhakaCharge ?? settings?.ShippingFee ?? 0;
        }

        private async Task<string> UniqueOrderNumberAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                string number = OrderNumberGenerator.Generate();
                if (!await _context.Orders.AnyAsync(o => o.OrderNumber == number)) return number;
            }

            return OrderNumberGenerator.Generate() + Random.Shared.Next(10, 99);
        }

        private static string? NormalizeQrPath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "/images/payments/bkash-qr.jpg";

            value = value.Trim();

            if (value.StartsWith("/") || value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return value;

            return "/images/payments/" + value;
        }

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}