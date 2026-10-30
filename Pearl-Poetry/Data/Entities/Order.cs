using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int? UserId { get; set; }

    public int? GuestUserId { get; set; }

    public int? ShippingAddressId { get; set; }

    public string ShipFullName { get; set; } = null!;

    public string? ShipEmail { get; set; }

    public string ShipPhone { get; set; } = null!;

    public string? ShipDistrict { get; set; }

    public string ShipAddress { get; set; } = null!;

    public string? ShipPostalCode { get; set; }

    public string? ShipCountry { get; set; }

    public string? ShipDeliveryNote { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? DeliveryCharge { get; set; }

    public decimal TotalAmount { get; set; }

    public string? CouponCode { get; set; }

    public string Status { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? CustomerNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual GuestUser? GuestUser { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderReturn> OrderReturns { get; set; } = new List<OrderReturn>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ShippingAddress? ShippingAddress { get; set; }

    public virtual User? User { get; set; }
}
