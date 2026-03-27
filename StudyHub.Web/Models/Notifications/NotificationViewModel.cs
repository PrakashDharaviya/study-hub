namespace StudyHub.Web.Models.Notifications;

/// <summary>
/// Represents a notification alert to be displayed in the user's navbar or notification center.
/// </summary>
public class NotificationViewModel
{
    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// e.g., "GroupInvite", "SystemAlert"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Helper to display time ago (e.g., "2 mins ago")
    /// </summary>
    public string TimeAgo => GetTimeAgo(CreatedAt);

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return dateTime.ToString("MMM dd");
    }
}

/// <summary>
/// Summary model for the Navbar Bell icon.
/// </summary>
public class NotificationSummaryViewModel
{
    public int UnreadCount { get; set; }
    public List<NotificationViewModel> RecentNotifications { get; set; } = new();
}