using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Groups;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the discovery, creation, modification, and membership of Study Groups.
/// Refactored to use IGroupService for clean architecture.
/// </summary>
[Authorize]
public class GroupsController : Controller
{
    private readonly IGroupService _groupService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GroupsController(IGroupService groupService, UserManager<ApplicationUser> userManager)
    {
        _groupService = groupService;
        _userManager = userManager;
    }

    // GET: /Groups
    [HttpGet]
    public async Task<IActionResult> Index(string? searchString, string? semester)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var groups = await _groupService.GetDiscoverGroupsAsync(searchString, semester, userId);

        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentSemester"] = semester;

        var distinctSemesters = await _groupService.GetDistinctSemestersAsync();
        ViewBag.Semesters = new SelectList(distinctSemesters);

        return View(groups);
    }

    // GET: /Groups/MyGroups
    [HttpGet]
    public async Task<IActionResult> MyGroups()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var groups = await _groupService.GetMyGroupsAsync(userId);

        return View(groups);
    }

    // GET: /Groups/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateGroupViewModel());
    }

    // POST: /Groups/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var newGroupId = await _groupService.CreateGroupAsync(model, userId);

        return RedirectToAction(nameof(Details), new { id = newGroupId });
    }

    // POST: /Groups/Join/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var success = await _groupService.JoinGroupAsync(id, userId);
        if (!success) return NotFound();

        return RedirectToAction(nameof(Details), new { id = id });
    }

    // GET: /Groups/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var viewModel = await _groupService.GetGroupDetailsAsync(id, userId, isPlatformAdmin);

        if (viewModel == null) return NotFound();

        return View(viewModel);
    }

    // ==========================================
    // NEW CRUD OPERATIONS (Edit & Delete)
    // ==========================================

    // GET: /Groups/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var model = await _groupService.GetGroupForEditAsync(id, userId, isPlatformAdmin);

        if (model == null) return Forbid();

        ViewBag.GroupId = id;
        return View(model);
    }

    // POST: /Groups/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateGroupViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        if (!ModelState.IsValid)
        {
            ViewBag.GroupId = id;
            return View(model);
        }

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var success = await _groupService.UpdateGroupAsync(id, model, userId, isPlatformAdmin);

        if (!success) return Forbid();

        return RedirectToAction(nameof(Details), new { id = id });
    }

    // GET: /Groups/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var model = await _groupService.GetGroupForEditAsync(id, userId, isPlatformAdmin);

        if (model == null) return Forbid();

        ViewBag.GroupId = id;
        return View(model);
    }

    // POST: /Groups/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var success = await _groupService.DeleteGroupAsync(id, userId, isPlatformAdmin);

        if (!success) return Forbid();

        return RedirectToAction(nameof(Index));
    }
    // POST: /Groups/RemoveMember
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(Guid groupId, string memberId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var success = await _groupService.RemoveMemberAsync(groupId, memberId, userId, isPlatformAdmin);

        if (!success) return Forbid(); // Either not an admin, or tried to remove another admin

        return RedirectToAction(nameof(Details), new { id = groupId });
    }
}