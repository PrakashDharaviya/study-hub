using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a user's membership within a specific study group.
/// Acts as a junction table with additional payload (Role, JoinedAt).
/// </summary>
public class GroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    public Guid StudyGroupId { get; set; }
    [ForeignKey(nameof(StudyGroupId))]
    public virtual StudyGroup StudyGroup { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Defines the user's permission level in the group. 
    /// Expected values: "Admin", "Member".
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Member";
}