namespace StudyHub.Web.Models.Groups;

/// <summary>
/// ViewModel for displaying the full details of a specific study group, 
/// including its members and shared resources.
/// </summary>
public class GroupDetailViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string TopicTags { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Determines if the current user can view the resources/members or if they need to join first.
    /// </summary>
    public bool IsCurrentUserMember { get; set; }

    /// <summary>
    /// "Admin" or "Member". Used to show/hide administrative buttons for the group creator.
    /// </summary>
    public string CurrentUserRole { get; set; } = string.Empty;

    /// <summary>
    /// Determines if the current user is a global Platform Administrator (God Mode).
    /// </summary>
    public bool IsPlatformAdmin { get; set; }

    /// <summary>
    /// NEW: The ID of the currently logged-in user. Used to determine resource ownership.
    /// </summary>
    public string CurrentUserId { get; set; } = string.Empty;

    public List<GroupMemberViewModel> Members { get; set; } = new List<GroupMemberViewModel>();
    public List<GroupResourceViewModel> Resources { get; set; } = new List<GroupResourceViewModel>();
}

public class GroupMemberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class GroupResourceViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Link", "PDF"
    public string UrlOrPath { get; set; } = string.Empty;

    /// <summary>
    /// NEW: The ID of the user who uploaded this resource.
    /// </summary>
    public string UploaderId { get; set; } = string.Empty;

    public string UploaderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPinned { get; set; }
}