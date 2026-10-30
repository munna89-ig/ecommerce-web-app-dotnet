namespace Pearl_Poetry.ViewModels.Shop
{
    // Customer-facing product page (the admin one lives in ViewModels.Admin.Product)
    public class ShopProductDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Sku { get; set; }

        public string? CategoryName { get; set; }

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        // stock of the product itself (used only when it has no variants)
        public int Stock { get; set; }

        public string CoverImage { get; set; } = "/images/categories/fashion.jpg";

        public List<string> Images { get; set; } = new();

        public List<ShopVariantViewModel> Variants { get; set; } = new();

        public bool InWishlist { get; set; }

        public bool HasVariants => Variants.Count > 0;

        // a group is only shown when at least one variant really has a value
        public bool HasColors => Variants.Any(v => v.Color != null);

        public bool HasSizes => Variants.Any(v => v.Size != null);
    }

    public class ShopVariantViewModel
    {
        public int Id { get; set; }

        // null when empty in the database
        public string? Color { get; set; }

        public string? Size { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; }
    }
}