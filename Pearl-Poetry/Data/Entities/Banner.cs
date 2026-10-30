using System;
using System.Collections.Generic;

namespace Pearl_Poetry.Data.Entities;

public partial class Banner
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string ImagePath { get; set; } = null!;

    public string? LinkUrl { get; set; }

    public string Position { get; set; } = null!;

    public int? SortOrder { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
