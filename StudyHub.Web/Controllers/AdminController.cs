using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Admin;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages platform-wide administration and statistics.
/// Restricted exclusively to users with the "PlatformAdmin" role.
/// </summary>
[Authorize(Roles = "PlatformAdmin")]
public class AdminController : Controller
{
    private readonly StudyHubDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(StudyHubDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Admin
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 1. Gather Platform Statistics
        var totalUsers = await _userManager.Users.CountAsync();
        var totalGroups = await _context.StudyGroups.CountAsync();
        var totalResources = await _context.Resources.CountAsync();

        // 2. Get Recent Groups (Top 5)
        var recentGroups = await _context.StudyGroups
            .Include(g => g.Members)
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .Take(5)
            .Select(g => new AdminRecentGroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Semester = g.Semester,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count
            })
            .ToListAsync();

        // 3. Map to ViewModel
        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalGroups = totalGroups,
            TotalResources = totalResources,
            RecentGroups = recentGroups
        };

        return View(viewModel);
    }
}