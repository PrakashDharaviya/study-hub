using StudyHub.Web.Models.Notifications;

namespace StudyHub.Web.Services.Interfaces;

/// <summary>
/// Defines the contract for managing group invitations and the join request workflow.
/// </summary>
public interface IInvitationService
{
    /// <summary>
    /// Sends an invitation to a user. Returns true if successful.
    /// </summary>
    Task<bool> SendInvitationAsync(Guid groupId, string inviterId, string inviteeEmail);

    /// <summary>
    /// Retrieves all pending invitations for a specific user.
    /// </summary>
    Task<IEnumerable<InvitationViewModel>> GetPendingInvitationsAsync(string userId);

    /// <summary>
    /// Accepts an invitation, adds the user to the group, and marks the invite as Accepted.
    /// </summary>
    Task<bool> AcceptInvitationAsync(Guid invitationId, string userId);

    /// <summary>
    /// Declines an invitation and marks it as Declined.
    /// </summary>
    Task<bool> DeclineInvitationAsync(Guid invitationId, string userId);

    /// <summary>
    /// Checks if a user is already a member or has a pending invitation to a group.
    /// </summary>
    Task<bool> IsUserInvitedOrMemberAsync(Guid groupId, string userId);
}