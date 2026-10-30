using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Checkout;

namespace Pearl_Poetry.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkout;

        public CheckoutController(ICheckoutService checkout)
        {
            _checkout = checkout;
        }

        // /Checkout                         => whole cart
        // /Checkout?productId=5&variantId=9&quantity=2  => "Buy now"
        [HttpGet]
        public async Task<IActionResult> Index(int? productId, int? variantId, int quantity = 1)
        {
            var model = await _checkout.StartAsync(productId, variantId, quantity);

            if (model == null)
            {
                return productId.HasValue
                    ? RedirectToAction("Details", "Product", new { id = productId })
                    : RedirectToAction("Index", "Cart");
            }

            return View(model);
        }

        // posted by the "Buy Now" button on the product page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyNow(int productId, int? variantId, int quantity = 1)
        {
            return RedirectToAction(nameof(Index), new { productId, variantId, quantity });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            bool isBkash = string.Equals(model.PaymentMethod, "bkash", StringComparison.OrdinalIgnoreCase);

            if (isBkash)
            {
                if (string.IsNullOrWhiteSpace(model.SenderPhone))
                    ModelState.AddModelError(nameof(model.SenderPhone), "Please enter the bKash number you paid from.");

                if (string.IsNullOrWhiteSpace(model.TransactionId))
                    ModelState.AddModelError(nameof(model.TransactionId), "Please enter the bKash transaction ID.");
            }

            if (!ModelState.IsValid)
            {
                if (!await _checkout.LoadDisplayAsync(model))
                    return RedirectToAction("Index", "Cart");

                return View("Index", model);
            }

            var result = await _checkout.PlaceOrderAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Could not place your order.");

                if (!await _checkout.LoadDisplayAsync(model))
                    return RedirectToAction("Index", "Cart");

                return View("Index", model);
            }

            return RedirectToAction(nameof(Success), new { orderNumber = result.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Success(string orderNumber)
        {
            var model = await _checkout.GetSuccessAsync(orderNumber);
            if (model == null) return NotFound();

            return View(model);
        }
    }
}