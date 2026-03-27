using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a shared file, link, or text note within a study group.
/// </summary>
public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty; [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The type of resource. Expected values: "Link", "PDF", "Document", "Image", "Note".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Stores the external URL (for links) or the relative file path (for uploads).
    /// </summary>
    [Required]
    public string UrlOrPath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPinned { get; set; } = false;

    // Navigation Properties

    [Required]
    public Guid StudyGroupId { get; set; }
    [ForeignKey(nameof(StudyGroupId))]
    public virtual StudyGroup StudyGroup { get; set; } = null!;

    [Required]
    public string UploaderId { get; set; } = string.Empty;

    [ForeignKey(nameof(UploaderId))]
    public virtual ApplicationUser Uploader { get; set; } = null!;
}