using Pearl_Poetry.Enums;

namespace Pearl_Poetry.Models.ViewModels.Admin
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public UserRole Role { get; set; }

        public string? ProfilePic { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
