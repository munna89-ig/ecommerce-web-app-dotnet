using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;
}
