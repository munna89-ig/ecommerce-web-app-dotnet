using System;
using System.Collections.Generic;

namespace Pearl_Poetry.ViewModels.Admin.Product
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Sku { get; set; }

        public string? ProductType { get; set; }

        public string? CategoryName { get; set; }

        public string? Description { get; set; }

        public string? ShortDescription { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public int Quantity { get; set; }

        public string? CoverImagePath { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<ProductImageViewModel> Images { get; set; }
            = new List<ProductImageViewModel>();

        public List<ProductVariantViewModel> Variants { get; set; }
            = new List<ProductVariantViewModel>();
    }

    public class ProductImageViewModel
    {
        public int Id { get; set; }

        public string? ImagePath { get; set; }

        public int SortOrder { get; set; }
    }

    public class ProductVariantViewModel
    {
        public int Id { get; set; }

        public string? Sku { get; set; }

        public string? Color { get; set; }

        public string? Size { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public int Quantity { get; set; }

        public bool IsActive { get; set; }
    }
}