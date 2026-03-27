using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudyHub.Web.Data.Entities;

/// <summary>
/// Represents a collaborative study group within the platform.
/// </summary>
public class StudyGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Semester { get; set; } = string.Empty; // e.g., "Fall 2024"

    [MaxLength(200)]
    public string TopicTags { get; set; } = string.Empty; // Comma-separated list for v1

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// The students who have joined this study group.
    /// </summary>
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    /// <summary>
    /// The shared resources (files, links, notes) posted in this group.
    /// </summary>
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}