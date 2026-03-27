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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call the base method first to configure Identity tables
        base.OnModelCreating(builder);

        // 1. Enforce unique membership (A user can only join a specific group once)
        builder.Entity<GroupMember>()
            .HasIndex(gm => new { gm.UserId, gm.StudyGroupId })
            .IsUnique();

        // 2. Prevent SQL Server "Multiple Cascade Paths" error
        // If a user is deleted, we don't automatically delete their resources to preserve group history.
        // If a group is deleted, the resources WILL be deleted (Cascade by default).
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
    }
}