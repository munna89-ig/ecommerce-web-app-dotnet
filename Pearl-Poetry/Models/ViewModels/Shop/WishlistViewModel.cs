namespace Pearl_Poetry.ViewModels.Shop
{
    public class WishlistViewModel
    {
        public List<WishlistItemViewModel> Items { get; set; } = new();
    }

    public class WishlistItemViewModel
    {
        public int ProductId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        // true => customer must pick color / size on the product page
        public bool HasVariants { get; set; }

        public bool InStock { get; set; }
    }
}