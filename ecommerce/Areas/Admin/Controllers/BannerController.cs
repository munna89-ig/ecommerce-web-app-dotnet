using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.ViewModels.Admin.Banners;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private const string HeroPosition = "homepage_hero";
        private const string ImageFolder = "banner";   // wwwroot/images/banner

        private readonly EcommerceSystemContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerController(EcommerceSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Admin/Banner
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners
                .AsNoTracking()
                .Where(b => b.Position == HeroPosition)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .ToListAsync();

            ViewBag.Today = LocalToday();
            return View(banners);
        }

        // GET: /Admin/Banner/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int nextOrder = (await _context.Banners
                .Where(b => b.Position == HeroPosition)
                .MaxAsync(b => (int?)b.SortOrder) ?? 0) + 1;

            return View("Form", new BannerFormViewModel { SortOrder = nextOrder, IsActive = true });
        }

        // POST: /Admin/Banner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BannerFormViewModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
                ModelState.AddModelError(nameof(model.ImageFile), "Please choose a banner image.");

            ValidateDates(model);

            if (!ModelState.IsValid)
                return View("Form", model);

            string? imagePath;
            try
            {
                imagePath = await FileUploadHelper.UploadImageAsync(model.ImageFile, _env.WebRootPath, ImageFolder);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View("Form", model);
            }

            var now = DateTime.UtcNow;

            _context.Banners.Add(new Banner
            {
                Title = Clean(model.Title),
                ImagePath = imagePath!,
                LinkUrl = Clean(model.LinkUrl),
                Position = HeroPosition,
                SortOrder = model.SortOrder,
                StartDate = model.StartDate?.Date,
                EndDate = model.EndDate?.Date,
                IsActive = model.IsActive,
                CreatedBy = CurrentUserId(),
                UpdatedBy = CurrentUserId(),
                CreatedAt = now,
                UpdatedAt = now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Banner added.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Banner/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return NotFound();

            return View("Form", new BannerFormViewModel
            {
                Id = b.Id,
                Title = b.Title,
                LinkUrl = b.LinkUrl,
                SortOrder = b.SortOrder ?? 0,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                IsActive = b.IsActive ?? true,
                ImagePath = b.ImagePath
            });
        }

        // POST: /Admin/Banner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BannerFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            var banner = await _context.Banners.FirstOrDefaultAsync(x => x.Id == id);
            if (banner == null) return NotFound();

            model.ImagePath = banner.ImagePath;   // for the preview if the form is shown again
            ValidateDates(model);

            if (!ModelState.IsValid)
                return View("Form", model);

            string? newImage;
            try
            {
                newImage = await FileUploadHelper.UploadImageAsync(model.ImageFile, _env.WebRootPath, ImageFolder);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View("Form", model);
            }

            string oldImage = banner.ImagePath;

            banner.Title = Clean(model.Title);
            banner.LinkUrl = Clean(model.LinkUrl);
            banner.SortOrder = model.SortOrder;
            banner.StartDate = model.StartDate?.Date;
            banner.EndDate = model.EndDate?.Date;
            banner.IsActive = model.IsActive;
            if (newImage != null) banner.ImagePath = newImage;
            banner.UpdatedBy = CurrentUserId();
            banner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (newImage != null) DeleteIfUploaded(oldImage);

            TempData["Success"] = "Banner updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Banner/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var banner = await _context.Banners.FirstOrDefaultAsync(x => x.Id == id);
            if (banner == null) return NotFound();

            banner.IsActive = !(banner.IsActive ?? true);
            banner.UpdatedBy = CurrentUserId();
            banner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = banner.IsActive == true ? "Banner is now visible." : "Banner hidden.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Banner/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FirstOrDefaultAsync(x => x.Id == id);
            if (banner == null) return NotFound();

            string image = banner.ImagePath;

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            DeleteIfUploaded(image);

            TempData["Success"] = "Banner deleted.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void ValidateDates(BannerFormViewModel model)
        {
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
                ModelState.AddModelError(nameof(model.EndDate), "End date cannot be before the start date.");
        }

        // removes only images uploaded from this page (guid names), never banner1.jpeg etc.
        private void DeleteIfUploaded(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (Guid.TryParse(Path.GetFileNameWithoutExtension(path), out _))
                FileUploadHelper.DeleteImage(path, _env.WebRootPath);
        }

        private int? CurrentUserId()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;

        private static DateTime LocalToday() => DateTime.UtcNow.AddHours(6).Date;   // Bangladesh

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}