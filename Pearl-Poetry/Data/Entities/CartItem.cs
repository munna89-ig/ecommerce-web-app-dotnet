using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class CartItem
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? GuestUserId { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GuestUser? GuestUser { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual ProductVariant? Variant { get; set; }
}
