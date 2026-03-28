using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the user-facing workflow for sending, accepting, or declining group invitations.
/// </summary>
[Authorize]
public class InvitationsController : Controller
{
    private readonly IInvitationService _invitationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGroupService _groupService;
    private readonly INotificationService _notificationService;

    public InvitationsController(
        IInvitationService invitationService,
        UserManager<ApplicationUser> userManager,
        IGroupService groupService,
        INotificationService notificationService)
    {
        _invitationService = invitationService;
        _userManager = userManager;
        _groupService = groupService;
        _notificationService = notificationService;
    }

    // POST: /Invitations/Send[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid groupId, string inviteeEmail)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // 1. Verify the current user is actually an Admin of the group
        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var groupDetails = await _groupService.GetGroupDetailsAsync(groupId, userId, isPlatformAdmin);

        if (groupDetails == null || (groupDetails.CurrentUserRole != "Admin" && !isPlatformAdmin))
        {
            return Forbid(); // Only Admins can send invites
        }

        // 2. Attempt to send the invite
        var success = await _invitationService.SendInvitationAsync(groupId, userId, inviteeEmail);

        if (!success)
        {
            TempData["InviteError"] = "Could not send invitation. User not found or already invited.";
        }
        else
        {
            TempData["InviteSuccess"] = "Invitation sent successfully!";
        }

        return RedirectToAction("Details", "Groups", new { id = groupId });
    }

    // GET: /Invitations/Respond/{id}
    [HttpGet]
    public async Task<IActionResult> Respond(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // Fetch all pending invites for this user
        var pendingInvites = await _invitationService.GetPendingInvitationsAsync(userId);

        // Find the specific invite they clicked on
        var invite = pendingInvites.FirstOrDefault(i => i.InvitationId == id);

        if (invite == null)
        {
            // If the invite doesn't exist or was already answered, redirect to home
            return RedirectToAction("Index", "Home");
        }

        // Mark all notifications as read since they are checking their inbox
        await _notificationService.MarkAllAsReadAsync(userId);

        return View(invite);
    }

    // POST: /Invitations/Accept/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var success = await _invitationService.AcceptInvitationAsync(id, userId);
        if (!success) return NotFound();

        return RedirectToAction("MyGroups", "Groups");
    }

    // POST: /Invitations/Decline/{id}[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var success = await _invitationService.DeclineInvitationAsync(id, userId);
        if (!success) return NotFound();

        return RedirectToAction("Index", "Groups");
    }
}