using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? MaxUsage { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public int? UsedCount { get; set; }

    public decimal? MinCustomerSpend { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual User? User { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
