using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Groups;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the discovery, creation, and membership of Study Groups.
/// </summary>
[Authorize]
public class GroupsController : Controller
{
    private readonly StudyHubDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GroupsController(StudyHubDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Groups
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var groups = await _context.StudyGroups
            .Include(g => g.Members)
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
                IsUserMember = g.Members.Any(m => m.UserId == userId)
            })
            .ToListAsync();

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
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = newGroup.Id });
    }

    // POST: /Groups/Join/{id}[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var groupExists = await _context.StudyGroups.AnyAsync(g => g.Id == id);
        if (!groupExists) return NotFound();

        var alreadyMember = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == id && m.UserId == userId);

        if (!alreadyMember)
        {
            var newMember = new GroupMember
            {
                StudyGroupId = id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = "Member"
            };

            _context.GroupMembers.Add(newMember);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }

    // GET: /Groups/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var group = await _context.StudyGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.Resources)
                .ThenInclude(r => r.Uploader)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        var currentUserMembership = group.Members.FirstOrDefault(m => m.UserId == userId);
        var isMember = currentUserMembership != null;

        var viewModel = new GroupDetailViewModel
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Semester = group.Semester,
            TopicTags = group.TopicTags,
            CreatedAt = group.CreatedAt,
            IsCurrentUserMember = isMember,
            CurrentUserRole = currentUserMembership?.Role ?? string.Empty,

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
                UploaderName = $"{r.Uploader.FirstName} {r.Uploader.LastName}".Trim(),
                CreatedAt = r.CreatedAt,
                IsPinned = r.IsPinned
            })
            .OrderByDescending(r => r.IsPinned)
            .ThenByDescending(r => r.CreatedAt)
            .ToList()
        };

        return View(viewModel);
    }
}