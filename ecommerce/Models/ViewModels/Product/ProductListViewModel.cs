using System.Collections.Generic;

namespace Pearl_Poetry.ViewModels.Admin.Product
{
    public class ProductListViewModel
    {
        public List<ProductListItemViewModel> Products { get; set; }
            = new List<ProductListItemViewModel>();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage =>
            CurrentPage > 1;

        public bool HasNextPage =>
            CurrentPage < TotalPages;
    }

    public class ProductListItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Sku { get; set; }

        public string? CategoryName { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public int Quantity { get; set; }

        public string? CoverImagePath { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }
    }
}