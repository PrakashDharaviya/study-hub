using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a lightweight activity update or short message within a study group's feed.
/// </summary>
public class ActivityFeed
{
    public Guid Id { get; set; } = Guid.NewGuid(); [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity. Expected values: "System" (e.g., User joined), "UserPost" (e.g., a short message).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "UserPost";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    [Required]
    public Guid StudyGroupId { get; set; }

    [ForeignKey(nameof(StudyGroupId))]
    public virtual StudyGroup StudyGroup { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}