using StudyHub.Web.Models.Groups;

namespace StudyHub.Web.Services.Interfaces;

/// <summary>
/// Defines the contract for Study Group business logic, abstracting EF Core 
/// away from the presentation layer (Controllers).
/// </summary>
public interface IGroupService
{
    Task<IEnumerable<GroupViewModel>> GetDiscoverGroupsAsync(string? searchString, string? semester, string userId);
    Task<IEnumerable<string>> GetDistinctSemestersAsync();
    Task<GroupDetailViewModel?> GetGroupDetailsAsync(Guid groupId, string userId, bool isPlatformAdmin);
    Task<Guid> CreateGroupAsync(CreateGroupViewModel model, string userId);
    Task<bool> JoinGroupAsync(Guid groupId, string userId);

    // CRUD OPERATIONS
    Task<CreateGroupViewModel?> GetGroupForEditAsync(Guid groupId, string userId, bool isPlatformAdmin);
    Task<bool> UpdateGroupAsync(Guid groupId, CreateGroupViewModel model, string userId, bool isPlatformAdmin);
    Task<bool> DeleteGroupAsync(Guid groupId, string userId, bool isPlatformAdmin);

    // USER SPECIFIC OPERATIONS
    Task<IEnumerable<GroupViewModel>> GetMyGroupsAsync(string userId);

    // ==========================================
    // NEW: MEMBER MANAGEMENT
    // ==========================================

    /// <summary>
    /// Removes a member from a study group. Only Admins can perform this action.
    /// </summary>
    Task<bool> RemoveMemberAsync(Guid groupId, string memberIdToRemove, string currentUserId, bool isPlatformAdmin);
}