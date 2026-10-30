using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Pearl_Poetry.ViewModels.Admin.Settings
{
    public class SettingsViewModel
    {
        // "general" | "delivery" | "contact"
        public string ActiveTab { get; set; } = "general";

        public DateTime? UpdatedAt { get; set; }

        // ================= GENERAL =================
        [Required(ErrorMessage = "Site name is required.")]
        [StringLength(255)]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "Pearl Poetry";

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; } = "BDT";

        [StringLength(255)]
        [Display(Name = "Copyright Text")]
        public string? CopyrightText { get; set; }

        // current logo (shown as preview)
        public string? LogoPath { get; set; }

        [Display(Name = "Upload new logo")]
        public IFormFile? LogoFile { get; set; }

        // ================= DELIVERY & PAYMENT =================
        [Range(0, 100000, ErrorMessage = "Enter a valid amount.")]
        [Display(Name = "Inside Dhaka Charge")]
        public decimal? InsideDhakaCharge { get; set; }

        [Range(0, 100000, ErrorMessage = "Enter a valid amount.")]
        [Display(Name = "Outside Dhaka Charge")]
        public decimal? OutsideDhakaCharge { get; set; }

        [Range(0, 100000, ErrorMessage = "Enter a valid amount.")]
        [Display(Name = "Default Delivery Charge")]
        public decimal? ShippingFee { get; set; }

        [Range(0, 100, ErrorMessage = "Tax must be between 0 and 100.")]
        [Display(Name = "Tax Rate (%)")]
        public decimal? TaxRate { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^(\+?88)?01[3-9]\d{8}$",
            ErrorMessage = "Enter a valid bKash number, e.g. 01712345678.")]
        [Display(Name = "bKash Number")]
        public string? BkashNumber { get; set; }

        // current QR (shown as preview)
        public string? BkashQrImage { get; set; }

        [Display(Name = "Upload new bKash QR")]
        public IFormFile? BkashQrFile { get; set; }

        // ================= CONTACT & SOCIAL =================
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(255)]
        [Display(Name = "Admin Email")]
        public string? AdminEmail { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Url(ErrorMessage = "Enter a full link starting with https://")]
        [StringLength(255)]
        [Display(Name = "Facebook")]
        public string? FacebookUrl { get; set; }

        [Url(ErrorMessage = "Enter a full link starting with https://")]
        [StringLength(255)]
        [Display(Name = "Instagram")]
        public string? InstagramUrl { get; set; }

        [Url(ErrorMessage = "Enter a full link starting with https://")]
        [StringLength(255)]
        [Display(Name = "YouTube")]
        public string? YoutubeUrl { get; set; }

        [Url(ErrorMessage = "Enter a full link starting with https://")]
        [StringLength(255)]
        [Display(Name = "WhatsApp")]
        public string? WhatsappUrl { get; set; }

        [Url(ErrorMessage = "Enter a full link starting with https://")]
        [StringLength(255)]
        [Display(Name = "Messenger")]
        public string? MessengerLink { get; set; }

        // "bkash-qr.jpg" or "/images/x.png" => usable url
        public static string? ImageUrl(string? value, string folder)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (value.StartsWith("/") || value.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return value;
            return $"/images/{folder}/{value}";
        }
    }
}