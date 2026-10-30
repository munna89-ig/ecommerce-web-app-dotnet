using Microsoft.AspNetCore.Http;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pearl_Poetry.ViewModels.Admin.Product
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Type")]
        public ProductType ProductType { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Discount Price")]
        [Range(0, double.MaxValue)]
        public decimal? DiscountPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Display(Name = "Featured Product")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        // Existing image
        public string? ExistingCoverImagePath { get; set; }

        // New image uploaded by admin
        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }
    }
}