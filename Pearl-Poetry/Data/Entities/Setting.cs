using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class Setting
{
    public int Id { get; set; }

    public string? SiteName { get; set; }

    public string? AdminEmail { get; set; }

    public string? Currency { get; set; }

    public decimal? TaxRate { get; set; }

    public decimal? ShippingFee { get; set; }

    public decimal? InsideDhakaCharge { get; set; }

    public decimal? OutsideDhakaCharge { get; set; }

    public string? BkashNumber { get; set; }

    public string? BkashQrImage { get; set; }

    public string? MessengerLink { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? LogoPath { get; set; }

    public string? ShortDescription { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? FacebookUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public string? YoutubeUrl { get; set; }

    public string? WhatsappUrl { get; set; }

    public string? CopyrightText { get; set; }
}
