using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Services;
using Pearl_Poetry.ViewModels.Account;

namespace Pearl_Poetry.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _env;
        private readonly IAuditLogger _audit;

        public AccountController(
            IAccountService accountService,
            IWebHostEnvironment env,
            IAuditLogger audit)
        {
            _accountService = accountService;
            _env = env;
            _audit = audit;
        }

        // =========================================================
        // LOGIN
        // =========================================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleDashboard();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _accountService.LoginAsync(model);

            if (user == null)
            {
                // wrong email / wrong password / inactive account
                await _audit.LogAsync(
                    "login_failed",
                    "users",
                    newValues: new { email = model.Email?.Trim().ToLower() });

                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                BuildPrincipal(user),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    AllowRefresh = true
                });

            // who logged in and when (created_at = login time)
            await _audit.LogAsync(
                "login",
                "users",
                user.Id,
                newValues: new
                {
                    name = user.Name,
                    email = user.Email,
                    role = user.Role.ToString(),
                    rememberMe = model.RememberMe
                },
                userId: user.Id);

            return RedirectToRoleDashboard(user.Role);
        }

        // =========================================================
        // REGISTER
        // =========================================================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleDashboard();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // the new user row is logged automatically as "create_user"
            bool success = await _accountService.RegisterAsync(model);

            if (!success)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Account created successfully. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // =========================================================
        // TEMPORARY: GENERATE ADMIN PASSWORD HASH
        // =========================================================
        [HttpGet]
        public IActionResult GenerateAdminHash()
        {
            return Content(PasswordHelper.HashPassword("Admin@123"));
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // log BEFORE signing out - afterwards we no longer know who it was
            if (TryGetUserId(out int userId))
            {
                await _audit.LogAsync(
                    "logout",
                    "users",
                    userId,
                    newValues: new { name = User.Identity?.Name },
                    userId: userId);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // =========================================================
        // PROFILE
        // GET: /Account/Profile?tab=overview|edit|password
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string? tab)
        {
            if (!TryGetUserId(out int userId))
                return RedirectToAction(nameof(Login));

            var model = await _accountService.GetProfileAsync(userId);

            if (model == null)
            {
                // account was deleted while the cookie was still alive
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            model.Edit = new ProfileEditViewModel { Name = model.Name, Phone = model.Phone };
            model.ActiveTab = tab is "edit" or "password" ? tab : "overview";

            return View(model);
        }

        // POST: /Account/UpdateProfile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Edit")] ProfileEditViewModel edit)
        {
            if (!TryGetUserId(out int userId))
                return RedirectToAction(nameof(Login));

            string? error = null;

            if (ModelState.IsValid)
            {
                // logged automatically as "update_user" (only the changed fields)
                var (success, err, user) =
                    await _accountService.UpdateProfileAsync(userId, edit, _env.WebRootPath);

                if (success && user != null)
                {
                    await RefreshSignInAsync(user);   // new name shows up immediately

                    TempData["SuccessMessage"] = "Your profile has been updated.";
                    return RedirectToAction(nameof(Profile), new { tab = "edit" });
                }

                error = err ?? "Could not update your profile.";
            }

            return await ShowProfileAgainAsync(userId, "edit", edit, editError: error);
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel password)
        {
            if (!TryGetUserId(out int userId))
                return RedirectToAction(nameof(Login));

            string? error = null;

            if (ModelState.IsValid)
            {
                // logged automatically as "update_user" with PasswordHash = "***"
                var (success, err) = await _accountService.ChangePasswordAsync(userId, password);

                if (success)
                {
                    TempData["SuccessMessage"] = "Your password has been changed.";
                    return RedirectToAction(nameof(Profile), new { tab = "password" });
                }

                error = err ?? "Could not change your password.";
            }

            return await ShowProfileAgainAsync(userId, "password", passwordError: error);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        // re-draws the profile page with the errors of the form that was posted
        private async Task<IActionResult> ShowProfileAgainAsync(
            int userId,
            string tab,
            ProfileEditViewModel? edit = null,
            string? editError = null,
            string? passwordError = null)
        {
            var model = await _accountService.GetProfileAsync(userId);
            if (model == null) return RedirectToAction(nameof(Login));

            model.Edit = edit ?? new ProfileEditViewModel { Name = model.Name, Phone = model.Phone };
            model.ActiveTab = tab;
            model.EditError = editError;
            model.PasswordError = passwordError;

            return View(nameof(Profile), model);
        }

        private static ClaimsPrincipal BuildPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        // keeps "Remember me" as it was, only the claims change
        private async Task RefreshSignInAsync(User user)
        {
            var current = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                BuildPrincipal(user),
                current.Properties ?? new AuthenticationProperties());
        }

        private bool TryGetUserId(out int userId)
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private IActionResult RedirectToRoleDashboard(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                UserRole.Seller => RedirectToAction("Index", "Dashboard"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        private IActionResult RedirectToRoleDashboard()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction(nameof(Login));

            if (User.IsInRole(UserRole.Admin.ToString()))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (User.IsInRole(UserRole.Seller.ToString()))
                return RedirectToAction("Index", "SellerDashboard");

            return RedirectToAction("Index", "Home");
        }
    }
}