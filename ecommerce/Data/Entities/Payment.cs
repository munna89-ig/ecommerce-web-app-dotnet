using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public int? GuestUserId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? SenderName { get; set; }

    public string? SenderPhone { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public int? VerifiedBy { get; set; }

    public string? VerificationNote { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GuestUser? GuestUser { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual User? VerifiedByNavigation { get; set; }
}
