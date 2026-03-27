namespace StudyHub.Web.Models.Groups;

/// <summary>
/// ViewModel for displaying a study group in lists and cards.
/// </summary>
public class GroupViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Semester { get; set; } = string.Empty;

    public string TopicTags { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The total number of students currently in this group.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Indicates if the currently logged-in user is already a member of this group.
    /// </summary>
    public bool IsUserMember { get; set; }
}