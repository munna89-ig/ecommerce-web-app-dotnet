using System;

namespace Pearl_Poetry.ViewModels.Admin.Dashboard
{
    public class RecentActivityViewModel
    {
        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string TimeAgo
        {
            get
            {
                var difference = DateTime.UtcNow - CreatedAt;

                if (difference.TotalSeconds < 60)
                    return "Just now";

                if (difference.TotalMinutes < 60)
                    return $"{(int)difference.TotalMinutes} minutes ago";

                if (difference.TotalHours < 24)
                    return $"{(int)difference.TotalHours} hours ago";

                if (difference.TotalDays < 2)
                    return "Yesterday";

                if (difference.TotalDays < 30)
                    return $"{(int)difference.TotalDays} days ago";

                return CreatedAt.ToLocalTime().ToString("dd MMM yyyy");
            }
        }
    }
}

