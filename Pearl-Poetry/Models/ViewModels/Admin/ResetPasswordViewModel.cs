using System.ComponentModel.DataAnnotations;
namespace Pearl_Poetry.Models.ViewModels.Admin
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }


        public string UserName { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password is required.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm the new password.")]
        [DataType(DataType.Password)]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match."
        )]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
