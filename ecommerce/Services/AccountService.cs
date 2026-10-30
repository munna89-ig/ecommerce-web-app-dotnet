using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.ViewModels.Account;

namespace Pearl_Poetry.Services
{
    public class AccountService(EcommerceSystemContext _context) : IAccountService
    {
       

       

        
        // LOGIN
    
        public async Task<User?> LoginAsync(LoginViewModel model)
        {
            var email = model.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
                return null;

            if (user.IsActive != true)
                return null;

            bool passwordValid = PasswordHelper.VerifyPassword(model.Password, user.PasswordHash);

            if (!passwordValid)
                return null;

            return user;
        }

        // REGISTER

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            var email = model.Email.Trim().ToLower();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email);

            if (emailExists)
                return false;

            var user = new User
            {
                Name = model.Name.Trim(),
                Email = email,
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),

                // Every normal registration is a customer
                Role = Enums.UserRole.Customer,

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        
        // PROFILE PAGE
        
        public async Task<ProfileViewModel?> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            if (user == null) return null;

            var orders = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            var activeStatuses = new[] { "pending", "confirmed", "processing", "shipped" };

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                ProfilePic = user.ProfilePic,
                CreatedAt = user.CreatedAt,

                TotalOrders = await orders.CountAsync(),

                ActiveOrders = await orders.CountAsync(o => activeStatuses.Contains(o.Status)),

                TotalSpent = await orders
                   .Where(o => o.PaymentStatus == "paid"
                     && o.Status != "cancelled"
                     && o.Status != "refunded")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                WishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId),

                RecentOrders = await orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new ProfileOrderViewModel
                    {
                        OrderNumber = o.OrderNumber,
                        CreatedAt = o.CreatedAt,
                        ItemCount = o.OrderItems.Sum(i => i.Quantity),
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus
                    })
                    .ToListAsync()
            };

            return model;
        }

       
        // UPDATE NAME / PHONE / PHOTO
      
        public async Task<(bool Success, string? Error, User? User)> UpdateProfileAsync(
            int userId, ProfileEditViewModel model, string webRootPath)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            if (user == null) return (false, "Your account was not found.", null);

            
            string? newPhoto = null;
            if (model.Photo != null && model.Photo.Length > 0)
            {
                try
                {
                    newPhoto = await FileUploadHelper.UploadImageAsync(model.Photo, webRootPath, "users");
                }
                catch (InvalidOperationException ex)
                {
                    return (false, ex.Message, null);
                }
            }

            string? oldPhoto = user.ProfilePic;

            user.Name = model.Name.Trim();
            user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();

            if (newPhoto != null)
                user.ProfilePic = newPhoto;
            else if (model.RemovePhoto)
                user.ProfilePic = null;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

           
            bool photoChanged = newPhoto != null || model.RemovePhoto;
            if (photoChanged && !string.IsNullOrWhiteSpace(oldPhoto) &&
                Guid.TryParse(Path.GetFileNameWithoutExtension(oldPhoto), out _))
            {
                FileUploadHelper.DeleteImage(oldPhoto, webRootPath);
            }

            return (true, null, user);
        }

       
        // CHANGE PASSWORD
    
        public async Task<(bool Success, string? Error)> ChangePasswordAsync(
            int userId, ChangePasswordViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            if (user == null) return (false, "Your account was not found.");

            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                return (false, "Your current password is not correct.");

            if (model.CurrentPassword == model.NewPassword)
                return (false, "The new password must be different from the current one.");

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, null);
        }
    }   
}       