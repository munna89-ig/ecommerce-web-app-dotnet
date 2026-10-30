using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class CouponUsage
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public int? UserId { get; set; }

    public int? GuestUserId { get; set; }

    public int OrderId { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual GuestUser? GuestUser { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? User { get; set; }
}
