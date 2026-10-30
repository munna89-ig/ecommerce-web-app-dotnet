using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Services.Interfaces;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private const int PageSize = 15;

        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: /Admin/Payment?search=&status=&method=&page=
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, string? method, int page = 1)
        {
            var model = await _paymentService.GetPaymentsAsync(search, status, method, page, PageSize);
            return View(model);
        }

        // POST: /Admin/Payment/Verify/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id, string? note, string? returnUrl)
        {
            var (success, message) = await _paymentService.VerifyAsync(
                id, GetCurrentUserId(), note, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData[success ? "Success" : "Error"] = message;
            return BackTo(returnUrl);
        }

        // POST: /Admin/Payment/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note, string? returnUrl)
        {
            var (success, message) = await _paymentService.RejectAsync(
                id, GetCurrentUserId(), note, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData[success ? "Success" : "Error"] = message;
            return BackTo(returnUrl);
        }

        // keeps the admin on the same filtered / paged list
        private IActionResult BackTo(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
    }
}