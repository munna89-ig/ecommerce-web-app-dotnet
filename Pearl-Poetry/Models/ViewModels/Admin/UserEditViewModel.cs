using System.ComponentModel.DataAnnotations;
using Pearl_Poetry.Enums;
namespace Pearl_Poetry.Models.ViewModels.Admin
{
    public class UserEditViewModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        public string? Phone { get; set; }


        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "User Role")]
        public UserRole Role { get; set; }


        [Display(Name = "Account Active")]
        public bool IsActive { get; set; }
    }
}
