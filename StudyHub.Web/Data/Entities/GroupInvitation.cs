using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents an invitation sent by a Group Admin to a user to join a specific study group.
/// </summary>
public class GroupInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudyGroupId { get; set; }
    [ForeignKey(nameof(StudyGroupId))]
    public virtual StudyGroup StudyGroup { get; set; } = null!; [Required]
    public string InviteeId { get; set; } = string.Empty;

    [ForeignKey(nameof(InviteeId))]
    public virtual ApplicationUser Invitee { get; set; } = null!;

    [Required]
    public string InviterId { get; set; } = string.Empty;

    [ForeignKey(nameof(InviterId))]
    public virtual ApplicationUser Inviter { get; set; } = null!;

    /// <summary>
    /// Expected values: "Pending", "Accepted", "Declined"
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }
}