using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Helpers
{
    // Small helpers shared by cart, wishlist, checkout and the product page
    // so that variant price / stock / label rules live in ONE place.
    public static class VariantHelper
    {
        // Returns null when the value is null / empty / whitespace,
        // so color & size are only ever shown when they really have a value.
        public static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // "Color: Red, Size: M" - parts that are null are skipped.
        // Returns null when there is nothing to show.
        public static string? BuildLabel(string? color, string? size)
        {
            var parts = new List<string>();

            var c = Clean(color);
            var s = Clean(size);

            if (c != null) parts.Add($"Color: {c}");
            if (s != null) parts.Add($"Size: {s}");

            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        // Variant price wins when the variant has one, otherwise product price.
        public static (decimal Price, decimal? DiscountPrice) ResolvePrice(
            Product product,
            ProductVariant? variant)
        {
            if (variant != null && variant.Price > 0)
                return (variant.Price, variant.DiscountPrice);

            return (product.Price, product.DiscountPrice);
        }

        public static int ResolveStock(Product product, ProductVariant? variant)
            => variant != null ? (variant.Quantity ?? 0) : (product.Quantity ?? 0);

        public static bool IsSellable(Product product, ProductVariant? variant)
            => product.IsActive == true
               && product.DeletedAt == null
               && (variant == null || variant.IsActive == true);
    }
}