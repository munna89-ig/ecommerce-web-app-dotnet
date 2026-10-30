using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.ViewModels.Admin.Settings;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly EcommerceSystemContext _context;
        private readonly IWebHostEnvironment _env;

        public SettingsController(EcommerceSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Admin/Settings?tab=general|delivery|contact
        [HttpGet]
        public async Task<IActionResult> Index(string? tab)
        {
            var s = await _context.Settings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync();

            var model = s == null
                ? new SettingsViewModel()
                : new SettingsViewModel
                {
                    SiteName = s.SiteName ?? "Pearl Poetry",
                    ShortDescription = s.ShortDescription,
                    Currency = s.Currency,
                    CopyrightText = s.CopyrightText,
                    LogoPath = s.LogoPath,

                    InsideDhakaCharge = s.InsideDhakaCharge,
                    OutsideDhakaCharge = s.OutsideDhakaCharge,
                    ShippingFee = s.ShippingFee,
                    TaxRate = s.TaxRate,
                    BkashNumber = s.BkashNumber,
                    BkashQrImage = s.BkashQrImage,

                    AdminEmail = s.AdminEmail,
                    Phone = s.Phone,
                    Address = s.Address,
                    FacebookUrl = s.FacebookUrl,
                    InstagramUrl = s.InstagramUrl,
                    YoutubeUrl = s.YoutubeUrl,
                    WhatsappUrl = s.WhatsappUrl,
                    MessengerLink = s.MessengerLink,

                    UpdatedAt = s.UpdatedAt
                };

            model.ActiveTab = tab is "delivery" or "contact" ? tab : "general";

            return View(model);
        }

        // POST: /Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            var settings = await _context.Settings.OrderBy(x => x.Id).FirstOrDefaultAsync();

            // current images are not posted - keep them for the preview
            model.LogoPath = settings?.LogoPath;
            model.BkashQrImage = settings?.BkashQrImage;
            model.UpdatedAt = settings?.UpdatedAt;

            if (!ModelState.IsValid)
                return View(model);

            // ---------- uploads first: if a file is wrong nothing is saved ----------
            string? newLogo = null;
            string? newQr = null;

            try
            {
                newLogo = await FileUploadHelper.UploadImageAsync(model.LogoFile, _env.WebRootPath, "settings");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
                model.ActiveTab = "general";
                return View(model);
            }

            try
            {
                newQr = await FileUploadHelper.UploadImageAsync(model.BkashQrFile, _env.WebRootPath, "payments");
            }
            catch (InvalidOperationException ex)
            {
                FileUploadHelper.DeleteImage(newLogo, _env.WebRootPath);   // undo the logo upload
                ModelState.AddModelError(nameof(model.BkashQrFile), ex.Message);
                model.ActiveTab = "delivery";
                return View(model);
            }

            // ---------- save ----------
            var now = DateTime.UtcNow;

            if (settings == null)
            {
                settings = new Setting { Id = 1, CreatedAt = now };
                _context.Settings.Add(settings);
            }

            string? oldLogo = settings.LogoPath;
            string? oldQr = settings.BkashQrImage;

            settings.SiteName = model.SiteName.Trim();
            settings.ShortDescription = Clean(model.ShortDescription);
            settings.Currency = Clean(model.Currency)?.ToUpper() ?? "BDT";
            settings.CopyrightText = Clean(model.CopyrightText);
            if (newLogo != null) settings.LogoPath = newLogo;

            settings.InsideDhakaCharge = model.InsideDhakaCharge ?? 0;
            settings.OutsideDhakaCharge = model.OutsideDhakaCharge ?? 0;
            settings.ShippingFee = model.ShippingFee ?? 0;
            settings.TaxRate = model.TaxRate ?? 0;
            settings.BkashNumber = Clean(model.BkashNumber);
            if (newQr != null) settings.BkashQrImage = newQr;

            settings.AdminEmail = Clean(model.AdminEmail);
            settings.Phone = Clean(model.Phone);
            settings.Address = Clean(model.Address);
            settings.FacebookUrl = Clean(model.FacebookUrl);
            settings.InstagramUrl = Clean(model.InstagramUrl);
            settings.YoutubeUrl = Clean(model.YoutubeUrl);
            settings.WhatsappUrl = Clean(model.WhatsappUrl);
            settings.MessengerLink = Clean(model.MessengerLink);

            settings.UpdatedAt = now;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = GetCurrentUserId(),
                Action = "settings_updated",
                EntityType = "settings",
                EntityId = settings.Id,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            // remove replaced images that were uploaded from this page (guid names only)
            if (newLogo != null) DeleteIfUploaded(oldLogo);
            if (newQr != null) DeleteIfUploaded(oldQr);

            TempData["Success"] = "Settings saved successfully.";
            return RedirectToAction(nameof(Index), new { tab = model.ActiveTab });
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void DeleteIfUploaded(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (Guid.TryParse(Path.GetFileNameWithoutExtension(path), out _))
                FileUploadHelper.DeleteImage(path, _env.WebRootPath);
        }

        private int? GetCurrentUserId()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}