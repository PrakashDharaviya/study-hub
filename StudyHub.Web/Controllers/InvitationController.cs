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

    public InvitationsController(
        IInvitationService invitationService,
        UserManager<ApplicationUser> userManager,
        IGroupService groupService)
    {
        _invitationService = invitationService;
        _userManager = userManager;
        _groupService = groupService;
    }

    // POST: /Invitations/Send
    [HttpPost]
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
            // For a production app, we would return a specific error message (e.g., "User not found" or "Already a member")
            // For the hackathon MVP, we will simply redirect back to the details page
            TempData["InviteError"] = "Could not send invitation. User not found or already invited.";
        }
        else
        {
            TempData["InviteSuccess"] = "Invitation sent successfully!";
        }

        return RedirectToAction("Details", "Groups", new { id = groupId });
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

    // POST: /Invitations/Decline/{id}
    [HttpPost]
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