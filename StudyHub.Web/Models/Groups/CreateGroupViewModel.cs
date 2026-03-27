using System.ComponentModel.DataAnnotations;

namespace StudyHub.Web.Models.Groups;

/// <summary>
/// ViewModel for creating a new study group. Enforces strict validation rules 
/// before saving to the database.
/// </summary>
public class CreateGroupViewModel
{
    [Required(ErrorMessage = "Group name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Group name must be between 3 and 100 characters.")]
    [Display(Name = "Group Name")]
    public string Name { get; set; } = string.Empty; [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Semester is required.")]
    [StringLength(50, ErrorMessage = "Semester cannot exceed 50 characters.")]
    [Display(Name = "Semester (e.g., Fall 2026)")]
    public string Semester { get; set; } = string.Empty; [StringLength(200, ErrorMessage = "Tags cannot exceed 200 characters.")]
    [Display(Name = "Topic Tags (Comma separated)")]
    public string TopicTags { get; set; } = string.Empty;
}