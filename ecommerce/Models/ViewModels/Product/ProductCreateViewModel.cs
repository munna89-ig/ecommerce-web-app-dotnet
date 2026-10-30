using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Pearl_Poetry.Models.Enums;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "Product title is required")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Product type is required")]
    [EnumDataType(typeof(ProductType), ErrorMessage = "Please select a valid product type")]
    public ProductType ProductType { get; set; }

    [Required(ErrorMessage = "Price is required")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    public int Quantity { get; set; }

    public decimal? DiscountPrice { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    public string Sku { get; set; }

    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Product image is required")]
    public IFormFile CoverImage { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
}