namespace Pearl_Poetry.ViewModels.Shop
{
    public class WishlistToggleRequest
    {
        public int ProductId { get; set; }
    }

    public class WishlistToggleResult
    {
        public bool Success { get; set; }

        // true => product is in the wishlist after the call
        public bool Active { get; set; }

        // guests have no wishlist: the browser sends them to the login page
        public bool RequiresLogin { get; set; }

        public string Message { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}