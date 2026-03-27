using StudyHub.Web.Models.Groups;

namespace StudyHub.Web.Services.Interfaces;

/// <summary>
/// Defines the contract for Study Group business logic, abstracting EF Core 
/// away from the presentation layer (Controllers).
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Retrieves all groups for the discovery dashboard, applying optional search and filters.
    /// </summary>
    Task<IEnumerable<GroupViewModel>> GetDiscoverGroupsAsync(string? searchString, string? semester, string userId);

    /// <summary>
    /// Retrieves a list of distinct semesters currently used by active groups.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctSemestersAsync();

    /// <summary>
    /// Retrieves the full details of a specific group, including members and resources.
    /// </summary>
    Task<GroupDetailViewModel?> GetGroupDetailsAsync(Guid groupId, string userId, bool isPlatformAdmin);

    /// <summary>
    /// Creates a new study group and assigns the creator as the Admin.
    /// </summary>
    Task<Guid> CreateGroupAsync(CreateGroupViewModel model, string userId);

    /// <summary>
    /// Adds a user to a study group as a standard Member.
    /// </summary>
    Task<bool> JoinGroupAsync(Guid groupId, string userId);

    // ==========================================
    //  CRUD OPERATIONS (Edit & Delete)
    // ==========================================

    /// <summary>
    /// Retrieves a group's data formatted for the Edit form. 
    /// Returns null if the group doesn't exist or the user is not an Admin.
    /// </summary>
    Task<CreateGroupViewModel?> GetGroupForEditAsync(Guid groupId, string userId, bool isPlatformAdmin);

    /// <summary>
    /// Updates an existing study group's details.
    /// </summary>
    Task<bool> UpdateGroupAsync(Guid groupId, CreateGroupViewModel model, string userId, bool isPlatformAdmin);

    /// <summary>
    /// Deletes a study group entirely. Only the Admin can perform this action.
    /// </summary>
    Task<bool> DeleteGroupAsync(Guid groupId, string userId, bool isPlatformAdmin);

    // ==========================================
    //  USER SPECIFIC OPERATIONS
    // ==========================================

    /// <summary>
    /// Retrieves all groups that the specified user is currently a member of.
    /// </summary>
    Task<IEnumerable<GroupViewModel>> GetMyGroupsAsync(string userId);
}