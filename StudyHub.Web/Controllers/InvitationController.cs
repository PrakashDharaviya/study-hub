using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the user-facing workflow for accepting or declining group invitations.
/// </summary>
[Authorize]
public class InvitationsController : Controller
{
    private readonly IInvitationService _invitationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvitationsController(
        IInvitationService invitationService,
        UserManager<ApplicationUser> userManager)
    {
        _invitationService = invitationService;
        _userManager = userManager;
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

        // Redirect to the user's list of groups
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

        // Redirect to the main discovery page
        return RedirectToAction("Index", "Groups");
    }
}