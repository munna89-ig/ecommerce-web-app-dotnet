using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.ViewModels.Account;

namespace Pearl_Poetry.Services
{
    public interface IAccountService
    {
        Task<User?> LoginAsync(LoginViewModel model);

        Task<bool> RegisterAsync(RegisterViewModel model);

        Task<User?> GetUserByIdAsync(int id);

        // ---------- profile ----------
        Task<ProfileViewModel?> GetProfileAsync(int userId);

        Task<(bool Success, string? Error, User? User)> UpdateProfileAsync(
            int userId, ProfileEditViewModel model, string webRootPath);

        Task<(bool Success, string? Error)> ChangePasswordAsync(
            int userId, ChangePasswordViewModel model);
    }
}