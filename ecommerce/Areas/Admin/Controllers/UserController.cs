using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Models;
using Pearl_Poetry.Models.ViewModels.Admin;
using Pearl_Poetry.ViewModels;
using System.Security.Claims;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly EcommerceSystemContext _context;

        public UserController(EcommerceSystemContext context)
        {
            _context = context;
        }

        // INDEX
        // GET: /Admin/User
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            UserRole? role,
            bool? isActive)
        {
            var query = _context.Users
                .AsNoTracking()
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.Phone != null && u.Phone.Contains(search)));
            }

            // Role filter
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            // Active/inactive filter
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedRole = role;
            ViewBag.SelectedStatus = isActive;

            return View(users);
        }

        // DETAILS
        // GET: /Admin/User/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // CREATE - GET
        // GET: /Admin/User/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new UserCreateViewModel
            {
                Role = UserRole.Customer,
                IsActive = true
            };

            return View(model);
        }

        // CREATE - POST
        // POST: /Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check duplicate email
            string email = model.Email.Trim().ToLower();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "A user with this email already exists."
                );

                return View(model);
            }

            // Create user
            var user = new User
            {
                Name = model.Name.Trim(),
                Email = email,
                Phone = string.IsNullOrWhiteSpace(model.Phone)
                    ? null
                    : model.Phone.Trim(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{user.Role} account created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // EDIT - GET
        // GET: /Admin/User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive ?? false   // FIX: NULL hole crash korbe na
            };

            return View(model);
        }

        // EDIT - POST
        // POST: /Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            int currentUserId = GetCurrentUserId();

            // Prevent admin from changing their own role
            if (user.Id == currentUserId && user.Role != model.Role)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "You cannot change your own administrator role."
                );

                return View(model);
            }

            // Prevent admin from deactivating themselves
            if (user.Id == currentUserId && !model.IsActive)
            {
                ModelState.AddModelError(
                    nameof(model.IsActive),
                    "You cannot deactivate your own account."
                );

                return View(model);
            }

            // Check duplicate email
            string email = model.Email.Trim().ToLower();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Id != id && u.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Another user already uses this email."
                );

                return View(model);
            }

            // Prevent removing last active admin
            if (user.Role == UserRole.Admin && model.Role != UserRole.Admin)
            {
                if (!await HasAnotherActiveAdmin(user.Id))
                {
                    ModelState.AddModelError(
                        nameof(model.Role),
                        "You cannot remove the last active administrator."
                    );

                    return View(model);
                }
            }

            if (user.Role == UserRole.Admin && !model.IsActive)
            {
                if (!await HasAnotherActiveAdmin(user.Id))
                {
                    ModelState.AddModelError(
                        nameof(model.IsActive),
                        "You cannot deactivate the last active administrator."
                    );

                    return View(model);
                }
            }

            // Update user
            user.Name = model.Name.Trim();
            user.Email = email;
            user.Phone = string.IsNullOrWhiteSpace(model.Phone)
                ? null
                : model.Phone.Trim();
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // DELETE - GET
        // GET: /Admin/User/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // DELETE - POST
        // POST: /Admin/User/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            int currentUserId = GetCurrentUserId();

            // Prevent deleting yourself
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting the last admin
            if (user.Role == UserRole.Admin)
            {
                if (!await HasAnotherActiveAdmin(user.Id))
                {
                    TempData["ErrorMessage"] = "You cannot delete the last active administrator.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // FIX: order/payment/review thakle database delete atkay, crash na kore message dekhabe
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "This user has orders or other records and cannot be deleted. Deactivate the account instead.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // TOGGLE STATUS
        // POST: /Admin/User/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            int currentUserId = GetCurrentUserId();

            // Prevent changing own status
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            bool isActiveNow = user.IsActive ?? false;

            // If disabling an admin, check last admin
            if (user.Role == UserRole.Admin && isActiveNow)
            {
                if (!await HasAnotherActiveAdmin(user.Id))
                {
                    TempData["ErrorMessage"] = "You cannot deactivate the last active administrator.";
                    return RedirectToAction(nameof(Index));
                }
            }

            user.IsActive = !isActiveNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = user.IsActive == true
                ? "User activated successfully."
                : "User deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // RESET PASSWORD - GET
        // GET: /Admin/User/ResetPassword/5
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.Email
            };

            return View(model);
        }

        // RESET PASSWORD - POST
        // POST: /Admin/User/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password reset successfully.";

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        // HELPER: CURRENT USER ID
        private int GetCurrentUserId()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userId, out int id) ? id : 0;
        }

        // HELPER: CHECK ANOTHER ACTIVE ADMIN
        private async Task<bool> HasAnotherActiveAdmin(int excludedUserId)
        {
            return await _context.Users
                .AnyAsync(u =>
                    u.Id != excludedUserId &&
                    u.Role == UserRole.Admin &&
                    (u.IsActive ?? false));
        }
    }
}