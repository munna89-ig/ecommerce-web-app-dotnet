using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Cart;

namespace Pearl_Poetry.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _cart.GetCartAsync());
        }

        // ---- the three methods below are called by JavaScript (JSON) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromBody] AddToCartRequest? request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(CartActionResult.Fail("Invalid request."));

            return Json(await _cart.AddAsync(request.ProductId, request.VariantId, request.Quantity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] UpdateCartRequest? request)
        {
            if (request == null || request.ItemId <= 0)
                return Json(CartActionResult.Fail("Invalid request."));

            return Json(await _cart.UpdateAsync(request.ItemId, request.Quantity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove([FromBody] RemoveCartRequest? request)
        {
            if (request == null || request.ItemId <= 0)
                return Json(CartActionResult.Fail("Invalid request."));

            return Json(await _cart.RemoveAsync(request.ItemId));
        }
    }
}