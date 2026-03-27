namespace StudyHub.Web.Models.Admin;

/// <summary>
/// ViewModel for the platform-wide Admin Dashboard.
/// Aggregates statistics across all users, groups, and resources.
/// </summary>
public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalGroups { get; set; }
    public int TotalResources { get; set; }

    /// <summary>
    /// A quick view of the most recently created study groups across the platform.
    /// </summary>
    public List<AdminRecentGroupViewModel> RecentGroups { get; set; } = new List<AdminRecentGroupViewModel>();
}

public class AdminRecentGroupViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}