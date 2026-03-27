using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Junction entity representing the many-to-many relationship 
/// between StudyGroups and Tags.
/// </summary>
public class GroupTag
{
    [Required]
    public Guid StudyGroupId { get; set; }

    [ForeignKey(nameof(StudyGroupId))]
    public virtual StudyGroup StudyGroup { get; set; } = null!;

    [Required]
    public Guid TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public virtual Tag Tag { get; set; } = null!;
}