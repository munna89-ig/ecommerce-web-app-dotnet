using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Enums;
using Pearl_Poetry.ViewModels.Coupon;

namespace Pearl_Poetry.Services
{
    public class CouponService : ICouponService
    {
        private readonly EcommerceSystemContext _context;

        public CouponService(EcommerceSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Coupon>> GetAllAsync(string? search, bool? isActive)
        {
            var query = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.Categories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Code.ToLower().Contains(term));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Coupon?> GetByIdAsync(int id)
        {
            return await _context.Coupons
                .Include(c => c.User)
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<User>> GetCustomersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Customer && u.DeletedAt == null)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // =========================================================
        // CREATE
        // =========================================================
        public async Task<(bool Success, string? Error)> CreateAsync(CouponCreateViewModel model)
        {
            var code = model.Code.Trim().ToUpper();

            bool codeExists = await _context.Coupons.AnyAsync(c => c.Code.ToUpper() == code);
            if (codeExists)
                return (false, "This coupon code already exists. Please choose another one.");

            if (model.ExpiryDate.HasValue && model.StartDate.HasValue &&
                model.ExpiryDate < model.StartDate)
                return (false, "Expiry date cannot be before the start date.");

            var coupon = new Coupon
            {
                Code = code,
                DiscountType = model.DiscountType,
                DiscountValue = model.DiscountValue,
                MinOrderAmount = model.MinOrderAmount,
                MaxDiscountAmount = model.MaxDiscountAmount,
                MaxUsage = model.MaxUsage,
                MaxUsagePerUser = model.MaxUsagePerUser,
                UsedCount = 0,
                StartDate = model.StartDate,
                ExpiryDate = model.ExpiryDate,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // who can use it
            var audienceError = await ApplyAudienceAsync(coupon, model.Audience, model.UserId, model.MinCustomerSpend);
            if (audienceError != null) return (false, audienceError);

            if (model.CategoryIds is { Count: > 0 })
            {
                var categories = await _context.Categories
                    .Where(c => model.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var category in categories)
                    coupon.Categories.Add(category);
            }

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // =========================================================
        // UPDATE
        // =========================================================
        public async Task<(bool Success, string? Error)> UpdateAsync(CouponEditViewModel model)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (coupon == null)
                return (false, "Coupon not found.");

            var code = model.Code.Trim().ToUpper();

            bool codeTaken = await _context.Coupons
                .AnyAsync(c => c.Id != model.Id && c.Code.ToUpper() == code);
            if (codeTaken)
                return (false, "This coupon code is already used by another coupon.");

            if (model.ExpiryDate.HasValue && model.StartDate.HasValue &&
                model.ExpiryDate < model.StartDate)
                return (false, "Expiry date cannot be before the start date.");

            // who can use it (checked first - nothing is saved on error)
            var audienceError = await ApplyAudienceAsync(coupon, model.Audience, model.UserId, model.MinCustomerSpend);
            if (audienceError != null) return (false, audienceError);

            coupon.Code = code;
            coupon.DiscountType = model.DiscountType;
            coupon.DiscountValue = model.DiscountValue;
            coupon.MinOrderAmount = model.MinOrderAmount;
            coupon.MaxDiscountAmount = model.MaxDiscountAmount;
            coupon.MaxUsage = model.MaxUsage;
            coupon.MaxUsagePerUser = model.MaxUsagePerUser;
            coupon.StartDate = model.StartDate;
            coupon.ExpiryDate = model.ExpiryDate;
            coupon.IsActive = model.IsActive;

            coupon.Categories.Clear();
            if (model.CategoryIds is { Count: > 0 })
            {
                var categories = await _context.Categories
                    .Where(c => model.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var category in categories)
                    coupon.Categories.Add(category);
            }

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return false;

            // Soft delete: coupons have no DeletedAt column, so we simply deactivate it.
            coupon.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================================
        // ELIGIBILITY (for the checkout "apply coupon" step)
        // =========================================================

        // only DELIVERED orders count - a pending order could still be cancelled
        public async Task<decimal> GetCustomerTotalSpentAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "delivered")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        }

        public async Task<(bool Eligible, string? Reason)> CheckCustomerEligibilityAsync(Coupon coupon, int? userId)
        {
            // 1) only for one customer
            if (coupon.UserId.HasValue && coupon.UserId != userId)
                return (false, "This coupon is not available for your account.");

            // 2) only for customers who already shopped above an amount
            if (coupon.MinCustomerSpend is > 0)
            {
                if (!userId.HasValue)
                    return (false, "Please log in to use this coupon.");

                decimal spent = await GetCustomerTotalSpentAsync(userId.Value);

                if (spent < coupon.MinCustomerSpend.Value)
                {
                    return (false,
                        $"This coupon is for customers who have shopped ৳{coupon.MinCustomerSpend.Value:N0} or more. " +
                        $"You have shopped ৳{spent:N0} so far.");
                }
            }

            // 3) everyone
            return (true, null);
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        // sets UserId / MinCustomerSpend from the chosen option, returns an error text or null
        private async Task<string?> ApplyAudienceAsync(Coupon coupon, string? audience, int? userId, decimal? minSpend)
        {
            switch (audience)
            {
                case "customer":
                    if (!userId.HasValue)
                        return "Please select a customer.";

                    bool userExists = await _context.Users
                        .AnyAsync(u => u.Id == userId.Value && u.DeletedAt == null);

                    if (!userExists)
                        return "Selected customer was not found.";

                    coupon.UserId = userId;
                    coupon.MinCustomerSpend = null;
                    return null;

                case "spend":
                    if (!minSpend.HasValue || minSpend <= 0)
                        return "Please enter the minimum total purchase amount.";

                    coupon.UserId = null;
                    coupon.MinCustomerSpend = minSpend;
                    return null;

                default: // "everyone"
                    coupon.UserId = null;
                    coupon.MinCustomerSpend = null;
                    return null;
            }
        }
    }
}