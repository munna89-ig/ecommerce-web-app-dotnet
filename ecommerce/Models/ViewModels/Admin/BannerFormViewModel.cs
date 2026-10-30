using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Pearl_Poetry.ViewModels.Admin.Banners
{
    public class BannerFormViewModel
    {
        public int Id { get; set; }

        public bool IsNew => Id == 0;

        [StringLength(150)]
        public string? Title { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^(/|https?://).*",
            ErrorMessage = "Link must start with / (inside the shop) or https://")]
        [Display(Name = "Link (optional)")]
        public string? LinkUrl { get; set; }

        [Range(0, 999, ErrorMessage = "Order must be between 0 and 999.")]
        [Display(Name = "Order")]
        public int SortOrder { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Show from")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Show until")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // current image (preview)
        public string? ImagePath { get; set; }

        [Display(Name = "Banner image")]
        public IFormFile? ImageFile { get; set; }

        // "banner1.jpeg" or "/images/banner/x.jpg" => usable url
        public static string ImageUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/images/categories/fashion.jpg";
            if (path.StartsWith("/") || path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
            return "/images/banner/" + path;
        }
    }
}