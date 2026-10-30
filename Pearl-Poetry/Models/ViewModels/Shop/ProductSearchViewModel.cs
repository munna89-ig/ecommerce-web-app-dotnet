namespace Pearl_Poetry.ViewModels.Shop
{
   
    public class ProductSearchViewModel
    {
        public string? Query { get; set; }

       
        public string? CategorySlug { get; set; }

        public string? CategoryName { get; set; }

     
        public string Sort { get; set; } = "newest";

        public int Page { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public int TotalItems { get; set; }

        // number of active products in the shop (sidebar "All Products")
        public int AllCount { get; set; }

        // categories whose name matched the search text
        public List<SearchCategoryItem> MatchedCategories { get; set; } = new();

        public List<SearchCategoryItem> Categories { get; set; } = new();

        public List<SearchProductItem> Products { get; set; } = new();

        public bool HasFilter => Query != null || CategorySlug != null;

        public string Heading =>
            Query != null ? $"Results for \"{Query}\""
            : CategoryName ?? "All Products";
    }

    public class SearchCategoryItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class SearchProductItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Sku { get; set; }

        public string CoverImage { get; set; } = "/images/categories/fashion.jpg";

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public bool HasVariants { get; set; }

        public bool InWishlist { get; set; }

        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice > 0 && DiscountPrice < Price;

        public decimal SellingPrice => HasDiscount ? DiscountPrice!.Value : Price;
    }
}