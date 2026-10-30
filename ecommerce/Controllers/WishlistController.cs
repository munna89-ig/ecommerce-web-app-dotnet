using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Shop;

namespace Pearl_Poetry.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlist;

        public WishlistController(IWishlistService wishlist)
        {
            _wishlist = wishlist;
        }

        // the page needs a login (guests are redirected to the login page)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _wishlist.GetWishlistAsync());
        }

        // JSON - guests get { requiresLogin: true } instead of a redirect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle([FromBody] WishlistToggleRequest? request)
        {
            if (request == null || request.ProductId <= 0)
                return Json(new WishlistToggleResult { Success = false, Message = "Invalid request." });

            return Json(await _wishlist.ToggleAsync(request.ProductId));
        }
    }
}