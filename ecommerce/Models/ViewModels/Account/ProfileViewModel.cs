using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Pearl_Poetry.Enums;

namespace Pearl_Poetry.ViewModels.Account
{
    // Whole "My Profile" page
    public class ProfileViewModel
    {
        // ---------- user ----------
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public UserRole Role { get; set; }

        public string? ProfilePic { get; set; }

        public DateTime? CreatedAt { get; set; }

        // "rahim.jpg" or "/images/users/x.jpg" => always a usable url
        public string? PhotoUrl =>
            string.IsNullOrWhiteSpace(ProfilePic) ? null
            : ProfilePic.StartsWith("/") || ProfilePic.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? ProfilePic
                : "/images/users/" + ProfilePic;

        // "Abdullah Al Nahian" => "AA"
        public string Initials
        {
            get
            {
                var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return "?";
                return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
            }
        }

        // ---------- stats ----------
        public int TotalOrders { get; set; }

        public int ActiveOrders { get; set; }

        public decimal TotalSpent { get; set; }

        public int WishlistCount { get; set; }

        public List<ProfileOrderViewModel> RecentOrders { get; set; } = new();

        // ---------- forms ----------
        public ProfileEditViewModel Edit { get; set; } = new();

        public ChangePasswordViewModel Password { get; set; } = new();

        // "overview" | "edit" | "password"
        public string ActiveTab { get; set; } = "overview";

        public string? EditError { get; set; }

        public string? PasswordError { get; set; }
    }

    public class ProfileOrderViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public int ItemCount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [RegularExpression(@"^(\+?88)?01[3-9]\d{8}$",
            ErrorMessage = "Enter a valid mobile number, e.g. 01712345678.")]
        public string? Phone { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? Photo { get; set; }

        public bool RemovePhoto { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your current password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a new password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the new password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}