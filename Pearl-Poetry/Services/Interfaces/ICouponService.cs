using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.ViewModels.Coupon;

namespace Pearl_Poetry.Services
{
    public interface ICouponService
    {
        Task<List<Coupon>> GetAllAsync(string? search, bool? isActive);

        Task<Coupon?> GetByIdAsync(int id);

        Task<List<Category>> GetCategoriesAsync();

        Task<List<User>> GetCustomersAsync();

        Task<(bool Success, string? Error)> CreateAsync(CouponCreateViewModel model);

        Task<(bool Success, string? Error)> UpdateAsync(CouponEditViewModel model);

        Task<bool> DeleteAsync(int id);

        // ---------- used when a customer applies a coupon ----------
        Task<decimal> GetCustomerTotalSpentAsync(int userId);

        Task<(bool Eligible, string? Reason)> CheckCustomerEligibilityAsync(Coupon coupon, int? userId);
    }
}