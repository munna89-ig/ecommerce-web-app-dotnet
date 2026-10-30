using System.ComponentModel.DataAnnotations;

namespace Pearl_Poetry.ViewModels.Coupon
{
    public class CouponCreateViewModel
    {
        [Required(ErrorMessage = "Coupon code is required.")]
        [MaxLength(50)]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Discount Type")]
        public string DiscountType { get; set; } = "percentage";

        [Required(ErrorMessage = "Discount value is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0.")]
        [Display(Name = "Discount Value")]
        public decimal DiscountValue { get; set; }

        [Display(Name = "Minimum Order Amount")]
        public decimal? MinOrderAmount { get; set; }

        [Display(Name = "Maximum Discount Amount")]
        public decimal? MaxDiscountAmount { get; set; }

        [Display(Name = "Total Usage Limit")]
        public int? MaxUsage { get; set; }

        [Display(Name = "Usage Limit Per User")]
        public int? MaxUsagePerUser { get; set; }

        [Display(Name = "Start Date")]
        public DateOnly? StartDate { get; set; }

        [Display(Name = "Expiry Date")]
        public DateOnly? ExpiryDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;


        [Display(Name = "Restrict To Specific Customer")]
        public int? UserId { get; set; }

        // "everyone" | "customer" | "spend"
        [Display(Name = "Who can use this coupon?")]
        public string Audience { get; set; } = "everyone";

        // only for "spend": customer's total delivered purchases must be >= this
        [Range(1, 99999999, ErrorMessage = "Enter an amount greater than 0.")]
        [Display(Name = "Minimum Total Purchase")]
        public decimal? MinCustomerSpend { get; set; }


        [Display(Name = "Applicable Categories")]
        public List<int> CategoryIds { get; set; } = new();
    }
}