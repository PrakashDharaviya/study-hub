using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Notifications;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Services;

/// <summary>
/// Concrete implementation of IInvitationService. Handles all business logic
/// for sending, accepting, and declining group invitations.
/// </summary>
public class InvitationService : IInvitationService
{
    private readonly StudyHubDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public InvitationService(
        StudyHubDbContext context,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<bool> SendInvitationAsync(Guid groupId, string inviterId, string inviteeEmail)
    {
        var invitee = await _userManager.FindByEmailAsync(inviteeEmail);
        if (invitee == null) return false; // User not found

        var group = await _context.StudyGroups.FindAsync(groupId);
        if (group == null) return false; // Group not found

        // Check if user is already a member or has a pending invite
        var alreadyInvitedOrMember = await IsUserInvitedOrMemberAsync(groupId, invitee.Id);
        if (alreadyInvitedOrMember) return false;

        var invitation = new GroupInvitation
        {
            StudyGroupId = groupId,
            InviterId = inviterId,
            InviteeId = invitee.Id,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Send a notification to the invitee
        var inviter = await _userManager.FindByIdAsync(inviterId);
        if (inviter == null) return false; // FIX: Null check added to resolve CS8602 warning

        var message = $"{inviter.FirstName} {inviter.LastName} invited you to join the group '{group.Name}'.";
        var actionUrl = $"/Invitations/Respond/{invitation.Id}";

        await _notificationService.CreateNotificationAsync(invitee.Id, message, "GroupInvite", actionUrl);

        return true;
    }

    public async Task<IEnumerable<InvitationViewModel>> GetPendingInvitationsAsync(string userId)
    {
        return await _context.GroupInvitations
            .Where(i => i.InviteeId == userId && i.Status == "Pending")
            .Include(i => i.StudyGroup)
            .Include(i => i.Inviter)
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvitationViewModel
            {
                InvitationId = i.Id,
                GroupId = i.StudyGroupId,
                GroupName = i.StudyGroup.Name,
                InviterName = $"{i.Inviter.FirstName} {i.Inviter.LastName}".Trim(),
                SentAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> AcceptInvitationAsync(Guid invitationId, string userId)
    {
        var invitation = await _context.GroupInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.InviteeId == userId && i.Status == "Pending");

        if (invitation == null) return false;

        // Add user to the group
        var newMember = new GroupMember
        {
            StudyGroupId = invitation.StudyGroupId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            Role = "Member"
        };
        _context.GroupMembers.Add(newMember);

        // Update invitation status
        invitation.Status = "Accepted";
        invitation.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeclineInvitationAsync(Guid invitationId, string userId)
    {
        var invitation = await _context.GroupInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.InviteeId == userId && i.Status == "Pending");

        if (invitation == null) return false;

        invitation.Status = "Declined";
        invitation.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserInvitedOrMemberAsync(Guid groupId, string userId)
    {
        var isMember = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == groupId && m.UserId == userId);

        if (isMember) return true;

        var hasPendingInvite = await _context.GroupInvitations
            .AnyAsync(i => i.StudyGroupId == groupId && i.InviteeId == userId && i.Status == "Pending");

        return hasPendingInvite;
    }
}