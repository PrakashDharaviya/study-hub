using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a system or user-generated alert for a specific student.
/// </summary>
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// e.g., "GroupInvite", "SystemAlert", "ResourceAdded"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "SystemAlert";

    /// <summary>
    /// Optional URL to redirect the user when they click the notification.
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}