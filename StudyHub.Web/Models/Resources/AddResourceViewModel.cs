using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudyHub.Web.Models.Resources;

/// <summary>
/// ViewModel for adding a new resource (file or link) to a specific study group.
/// </summary>
public class AddResourceViewModel
{
    [Required]
    public Guid StudyGroupId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty; [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Expected values: "Link", "PDF", "Document", "Image", "Note"
    /// </summary>
    [Required(ErrorMessage = "Please select a resource type.")]
    [Display(Name = "Resource Type")]
    public string ResourceType { get; set; } = "Link";

    [Display(Name = "External Link (URL)")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string? Url { get; set; }
    [Display(Name = "Upload File")]
    public IFormFile? UploadedFile { get; set; }
}