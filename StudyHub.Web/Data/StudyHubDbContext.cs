using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data.Entities;

namespace StudyHub.Web.Data;

/// <summary>
/// The primary database context for StudyHub, integrating ASP.NET Core Identity 
/// with our custom domain entities.
/// </summary>
public class StudyHubDbContext : IdentityDbContext<ApplicationUser>
{
    public StudyHubDbContext(DbContextOptions<StudyHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudyGroup> StudyGroups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Resource> Resources { get; set; }

    // DbSets for Schema Expansion (Phase 1)
    public DbSet<Tag> Tags { get; set; }
    public DbSet<GroupTag> GroupTags { get; set; }
    public DbSet<ActivityFeed> ActivityFeeds { get; set; }

    // DbSets for Schema Expansion (Phase 7 - Access & Invitations)
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<GroupInvitation> GroupInvitations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call the base method first to configure Identity tables
        base.OnModelCreating(builder);

        // 1. Enforce unique membership (A user can only join a specific group once)
        builder.Entity<GroupMember>()
            .HasIndex(gm => new { gm.UserId, gm.StudyGroupId })
            .IsUnique();

        // 2. Prevent SQL Server "Multiple Cascade Paths" error for Resources
        builder.Entity<Resource>()
            .HasOne(r => r.Uploader)
            .WithMany(u => u.UploadedResources)
            .HasForeignKey(r => r.UploaderId)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Explicitly configure GroupMember relationships
        builder.Entity<GroupMember>()
            .HasOne(gm => gm.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMember>()
            .HasOne(gm => gm.StudyGroup)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Configure GroupTag Composite Key and Relationships
        builder.Entity<GroupTag>()
            .HasKey(gt => new { gt.StudyGroupId, gt.TagId });

        builder.Entity<GroupTag>()
            .HasOne(gt => gt.StudyGroup)
            .WithMany(g => g.GroupTags)
            .HasForeignKey(gt => gt.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupTag>()
            .HasOne(gt => gt.Tag)
            .WithMany(t => t.GroupTags)
            .HasForeignKey(gt => gt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. Prevent SQL Server "Multiple Cascade Paths" error for ActivityFeed
        builder.Entity<ActivityFeed>()
            .HasOne(af => af.User)
            .WithMany(u => u.ActivityFeeds)
            .HasForeignKey(af => af.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActivityFeed>()
            .HasOne(af => af.StudyGroup)
            .WithMany(g => g.ActivityFeeds)
            .HasForeignKey(af => af.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6. Prevent SQL Server "Multiple Cascade Paths" error for GroupInvitations
        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.Invitee)
            .WithMany(u => u.ReceivedInvitations)
            .HasForeignKey(gi => gi.InviteeId)
            .OnDelete(DeleteBehavior.Restrict); // Do not cascade delete if user is deleted

        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.Inviter)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(gi => gi.InviterId)
            .OnDelete(DeleteBehavior.Restrict); // Do not cascade delete if user is deleted

        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.StudyGroup)
            .WithMany(g => g.Invitations)
            .HasForeignKey(gi => gi.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade); // Delete invites if group is deleted
    }
}