using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class OrderReturn
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? OrderItemId { get; set; }

    public int? UserId { get; set; }

    public int? GuestUserId { get; set; }

    public string? Reason { get; set; }

    public int Quantity { get; set; }

    public decimal RefundAmount { get; set; }

    public string Status { get; set; } = null!;

    public int? HandledBy { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GuestUser? GuestUser { get; set; }

    public virtual User? HandledByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual OrderItem? OrderItem { get; set; }

    public virtual User? User { get; set; }
}
