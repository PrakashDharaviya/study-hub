using System.ComponentModel.DataAnnotations;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a reusable topic tag (e.g., "Data Structures", "Organic Chemistry") 
/// that can be assigned to multiple study groups.
/// </summary>
public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid(); [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation Property

    /// <summary>
    /// The junction records linking this tag to various study groups.
    /// </summary>
    public virtual ICollection<GroupTag> GroupTags { get; set; } = new List<GroupTag>();
}