namespace StudyHub.Web.Models.Notifications;

/// <summary>
/// ViewModel representing a group invitation for the user's inbox.
/// </summary>
public class InvitationViewModel
{
    public Guid InvitationId { get; set; }

    public Guid GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string InviterName { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    /// <summary>
    /// Helper to display time ago (e.g., "5 mins ago")
    /// </summary>
    public string TimeAgo => GetTimeAgo(SentAt);

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return dateTime.ToString("MMM dd");
    }
}