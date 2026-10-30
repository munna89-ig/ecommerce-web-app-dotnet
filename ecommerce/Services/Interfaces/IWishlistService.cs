using Pearl_Poetry.ViewModels.Shop;

namespace Pearl_Poetry.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<WishlistViewModel> GetWishlistAsync();

        Task<int> GetCountAsync();

        // ids of the products in the logged-in user's wishlist (empty for guests)
        Task<HashSet<int>> GetProductIdsAsync();

        // add when missing, remove when present
        Task<WishlistToggleResult> ToggleAsync(int productId);
    }
}