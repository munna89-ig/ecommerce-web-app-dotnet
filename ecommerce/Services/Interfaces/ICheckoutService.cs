using Pearl_Poetry.ViewModels.Checkout;

namespace Pearl_Poetry.Services.Interfaces
{
    public interface ICheckoutService
    {
        // First visit of the checkout page. productId => "Buy now", otherwise the cart.
        // Returns null when there is nothing to check out.
        Task<CheckoutViewModel?> StartAsync(int? productId, int? variantId, int quantity);

        // Re-fills the display-only part (items, totals, bKash info) after a POST.
        // Returns false when there is nothing to check out any more.
        Task<bool> LoadDisplayAsync(CheckoutViewModel model);

        Task<PlaceOrderResult> PlaceOrderAsync(CheckoutViewModel model);

        // null when the order does not exist or does not belong to the current shopper
        Task<OrderSuccessViewModel?> GetSuccessAsync(string orderNumber);
    }
}