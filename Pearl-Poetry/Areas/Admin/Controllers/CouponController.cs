using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Services;
using Pearl_Poetry.ViewModels.Coupon;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET: /Admin/Coupon
        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? isActive)
        {
            var coupons = await _couponService.GetAllAsync(search, isActive);

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;

            return View(coupons);
        }

        // GET: /Admin/Coupon/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new CouponCreateViewModel());
        }

        // POST: /Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var (success, error) = await _couponService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Could not create the coupon.");
                await LoadDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = $"Coupon \"{model.Code}\" was created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Coupon/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon == null) return NotFound();

            var model = new CouponEditViewModel
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                MaxUsage = coupon.MaxUsage,
                MaxUsagePerUser = coupon.MaxUsagePerUser,
                StartDate = coupon.StartDate,
                ExpiryDate = coupon.ExpiryDate,
                IsActive = coupon.IsActive ?? true,
                UserId = coupon.UserId,

                // which "Who can use this coupon?" option is selected
                Audience = coupon.UserId.HasValue ? "customer"
                         : coupon.MinCustomerSpend > 0 ? "spend"
                         : "everyone",
                MinCustomerSpend = coupon.MinCustomerSpend,

                CategoryIds = coupon.Categories.Select(c => c.Id).ToList(),
                UsedCount = coupon.UsedCount ?? 0
            };

            await LoadDropdownsAsync();
            return View(model);
        }

        // POST: /Admin/Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CouponEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var (success, error) = await _couponService.UpdateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Could not update the coupon.");
                await LoadDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = $"Coupon \"{model.Code}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Coupon/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _couponService.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "Coupon not found.";
            }
            else
            {
                TempData["Success"] = "Coupon was deactivated.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            ViewBag.Customers = await _couponService.GetCustomersAsync();
            ViewBag.Categories = await _couponService.GetCategoriesAsync();
        }
    }
}