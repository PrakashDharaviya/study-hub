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
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Semester { get; set; } = string.Empty; // e.g., "Fall 2026"

    [MaxLength(200)]
    public string TopicTags { get; set; } = string.Empty; // Legacy comma-separated list

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

    /// <summary>
    /// The relational tags assigned to this group.
    /// </summary>
    public virtual ICollection<GroupTag> GroupTags { get; set; } = new List<GroupTag>();

    /// <summary>
    /// The activity feed events associated with this group.
    /// </summary>
    public virtual ICollection<ActivityFeed> ActivityFeeds { get; set; } = new List<ActivityFeed>();

    /// <summary>
    /// The invitations sent out for this specific group.
    /// </summary>
    public virtual ICollection<GroupInvitation> Invitations { get; set; } = new List<GroupInvitation>();
}