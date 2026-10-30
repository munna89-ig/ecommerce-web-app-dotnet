using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? VariantId { get; set; }

    public string ProductTitle { get; set; } = null!;

    public string? VariantLabel { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderReturn> OrderReturns { get; set; } = new List<OrderReturn>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ProductVariant? Variant { get; set; }
}
