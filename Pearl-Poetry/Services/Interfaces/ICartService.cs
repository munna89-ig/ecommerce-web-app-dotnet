using Pearl_Poetry.ViewModels.Cart;

namespace Pearl_Poetry.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync();

        // total pieces in the cart (header badge)
        Task<int> GetCountAsync();

        Task<CartActionResult> AddAsync(int productId, int? variantId, int quantity);

        Task<CartActionResult> UpdateAsync(int itemId, int quantity);

        Task<CartActionResult> RemoveAsync(int itemId);
    }
}