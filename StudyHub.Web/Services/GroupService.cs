using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Groups;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Services;

/// <summary>
/// Concrete implementation of IGroupService. Handles all EF Core database 
/// operations for Study Groups, Memberships, and related Activity Feeds.
/// </summary>
public class GroupService : IGroupService
{
    private readonly StudyHubDbContext _context;

    public GroupService(StudyHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GroupViewModel>> GetDiscoverGroupsAsync(string? searchString, string? semester, string userId)
    {
        var query = _context.StudyGroups
            .Include(g => g.Members)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(g => g.Name.Contains(searchString) || g.TopicTags.Contains(searchString));
        }

        if (!string.IsNullOrWhiteSpace(semester))
        {
            query = query.Where(g => g.Semester == semester);
        }

        return await query
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Semester = g.Semester,
                TopicTags = g.TopicTags,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count,
                IsUserMember = g.Members.Any(m => m.UserId == userId)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctSemestersAsync()
    {
        return await _context.StudyGroups
            .Select(g => g.Semester)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<GroupDetailViewModel?> GetGroupDetailsAsync(Guid groupId, string userId, bool isPlatformAdmin)
    {
        var group = await _context.StudyGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.Resources)
                .ThenInclude(r => r.Uploader)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return null;

        var currentUserMembership = group.Members.FirstOrDefault(m => m.UserId == userId);
        var isMember = currentUserMembership != null;

        return new GroupDetailViewModel
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Semester = group.Semester,
            TopicTags = group.TopicTags,
            CreatedAt = group.CreatedAt,
            IsCurrentUserMember = isMember,
            CurrentUserRole = currentUserMembership?.Role ?? string.Empty,
            IsPlatformAdmin = isPlatformAdmin,
            CurrentUserId = userId, // NEW: Pass the current user's ID to the view

            Members = group.Members.Select(m => new GroupMemberViewModel
            {
                UserId = m.UserId,
                FullName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
                Role = m.Role,
                JoinedAt = m.JoinedAt
            })
            .OrderByDescending(m => m.Role == "Admin")
            .ThenBy(m => m.JoinedAt)
            .ToList(),

            Resources = group.Resources.Select(r => new GroupResourceViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Type = r.Type,
                UrlOrPath = r.UrlOrPath,
                UploaderId = r.UploaderId, // NEW: Pass the uploader's ID to the view
                UploaderName = $"{r.Uploader.FirstName} {r.Uploader.LastName}".Trim(),
                CreatedAt = r.CreatedAt,
                IsPinned = r.IsPinned
            })
            .OrderByDescending(r => r.IsPinned)
            .ThenByDescending(r => r.CreatedAt)
            .ToList()
        };
    }

    public async Task<Guid> CreateGroupAsync(CreateGroupViewModel model, string userId)
    {
        var newGroup = new StudyGroup
        {
            Name = model.Name,
            Description = model.Description,
            Semester = model.Semester,
            TopicTags = model.TopicTags,
            CreatedAt = DateTime.UtcNow
        };

        _context.StudyGroups.Add(newGroup);

        var groupMember = new GroupMember
        {
            StudyGroupId = newGroup.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            Role = "Admin"
        };

        _context.GroupMembers.Add(groupMember);

        var activity = new ActivityFeed
        {
            StudyGroupId = newGroup.Id,
            UserId = userId,
            Content = "Group was created.",
            Type = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityFeeds.Add(activity);

        await _context.SaveChangesAsync();
        return newGroup.Id;
    }

    public async Task<bool> JoinGroupAsync(Guid groupId, string userId)
    {
        var groupExists = await _context.StudyGroups.AnyAsync(g => g.Id == groupId);
        if (!groupExists) return false;

        var alreadyMember = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == groupId && m.UserId == userId);

        if (alreadyMember) return true;

        var newMember = new GroupMember
        {
            StudyGroupId = groupId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            Role = "Member"
        };

        _context.GroupMembers.Add(newMember);

        var activity = new ActivityFeed
        {
            StudyGroupId = groupId,
            UserId = userId,
            Content = "Joined the study group.",
            Type = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityFeeds.Add(activity);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CreateGroupViewModel?> GetGroupForEditAsync(Guid groupId, string userId, bool isPlatformAdmin)
    {
        var group = await _context.StudyGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return null;

        // Security: Only Group Admins OR Platform Admins can edit
        var isGroupAdmin = group.Members.Any(m => m.UserId == userId && m.Role == "Admin");
        if (!isPlatformAdmin && !isGroupAdmin) return null;

        return new CreateGroupViewModel
        {
            Name = group.Name,
            Description = group.Description,
            Semester = group.Semester,
            TopicTags = group.TopicTags
        };
    }

    public async Task<bool> UpdateGroupAsync(Guid groupId, CreateGroupViewModel model, string userId, bool isPlatformAdmin)
    {
        var group = await _context.StudyGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return false;

        // Security: Only Group Admins OR Platform Admins can edit
        var isGroupAdmin = group.Members.Any(m => m.UserId == userId && m.Role == "Admin");
        if (!isPlatformAdmin && !isGroupAdmin) return false;

        group.Name = model.Name;
        group.Description = model.Description;
        group.Semester = model.Semester;
        group.TopicTags = model.TopicTags;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGroupAsync(Guid groupId, string userId, bool isPlatformAdmin)
    {
        var group = await _context.StudyGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return false;

        // Security: Only Group Admins OR Platform Admins can delete
        var isGroupAdmin = group.Members.Any(m => m.UserId == userId && m.Role == "Admin");
        if (!isPlatformAdmin && !isGroupAdmin) return false;

        _context.StudyGroups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<GroupViewModel>> GetMyGroupsAsync(string userId)
    {
        return await _context.StudyGroups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Semester = g.Semester,
                TopicTags = g.TopicTags,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count,
                IsUserMember = true
            })
            .ToListAsync();
    }

    public async Task<bool> RemoveMemberAsync(Guid groupId, string memberIdToRemove, string currentUserId, bool isPlatformAdmin)
    {
        var group = await _context.StudyGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return false;

        // Security: Only Group Admins OR Platform Admins can remove members
        var isGroupAdmin = group.Members.Any(m => m.UserId == currentUserId && m.Role == "Admin");
        if (!isPlatformAdmin && !isGroupAdmin) return false;

        // Find the member to remove
        var memberToRemove = group.Members.FirstOrDefault(m => m.UserId == memberIdToRemove);

        // Security: Cannot remove someone who isn't a member, and cannot remove an Admin
        if (memberToRemove == null || memberToRemove.Role == "Admin") return false;

        _context.GroupMembers.Remove(memberToRemove);

        // Log Activity
        var activity = new ActivityFeed
        {
            StudyGroupId = groupId,
            UserId = currentUserId,
            Content = "A member was removed from the group.",
            Type = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityFeeds.Add(activity);

        await _context.SaveChangesAsync();
        return true;
    }

}