using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Groups;
using StudyHub.Web.Models.Resources;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the uploading of files, sharing of links, and deletion of resources within Study Groups.
/// </summary>
[Authorize]
public class ResourcesController : Controller
{
    private readonly StudyHubDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ResourcesController(
        StudyHubDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // GET: /Resources/Add/{groupId}[HttpGet]
    public async Task<IActionResult> Add(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");

        // Security Check: Ensure the user is a Group Admin OR a Platform Admin
        var isAdmin = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == id && m.UserId == userId && m.Role == "Admin");

        if (!isAdmin && !isPlatformAdmin)
        {
            return Forbid(); // Block regular members from accessing the add resource page
        }

        var model = new AddResourceViewModel
        {
            StudyGroupId = id
        };

        return View(model);
    }

    // POST: /Resources/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddResourceViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        if (model.ResourceType == "Link" && string.IsNullOrWhiteSpace(model.Url))
        {
            ModelState.AddModelError("Url", "Please provide a valid URL for the link.");
        }
        else if (model.ResourceType != "Link" && model.UploadedFile == null)
        {
            ModelState.AddModelError("UploadedFile", "Please select a file to upload.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");

        // Security Check: Double-check Admin status on POST
        var isAdmin = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == model.StudyGroupId && m.UserId == userId && m.Role == "Admin");

        if (!isAdmin && !isPlatformAdmin) return Forbid(); // Block unauthorized POST requests

        string finalUrlOrPath = string.Empty;

        // Handle File Upload
        if (model.ResourceType != "Link" && model.UploadedFile != null)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "resources");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.UploadedFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.UploadedFile.CopyToAsync(fileStream);
            }

            finalUrlOrPath = "/uploads/resources/" + uniqueFileName;
        }
        else
        {
            finalUrlOrPath = model.Url!;
        }

        var newResource = new Resource
        {
            StudyGroupId = model.StudyGroupId,
            UploaderId = userId,
            Title = model.Title,
            Description = model.Description ?? string.Empty,
            Type = model.ResourceType,
            UrlOrPath = finalUrlOrPath,
            CreatedAt = DateTime.UtcNow,
            IsPinned = false
        };

        _context.Resources.Add(newResource);

        var activity = new ActivityFeed
        {
            StudyGroupId = model.StudyGroupId,
            UserId = userId,
            Content = $"Shared a new {model.ResourceType.ToLower()}: {model.Title}",
            Type = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityFeeds.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Groups", new { id = model.StudyGroupId });
    }

    // GET: /Resources/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var resource = await _context.Resources
            .Include(r => r.StudyGroup)
                .ThenInclude(g => g.Members)
            .Include(r => r.Uploader)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resource == null) return NotFound();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var isAdmin = resource.StudyGroup.Members.Any(m => m.UserId == userId && m.Role == "Admin");
        var isUploader = resource.UploaderId == userId;

        // Security: Only Uploader, Group Admin, OR Platform Admin can delete
        if (!isAdmin && !isUploader && !isPlatformAdmin) return Forbid();

        var viewModel = new GroupResourceViewModel
        {
            Id = resource.Id,
            Title = resource.Title,
            Description = resource.Description,
            Type = resource.Type,
            UrlOrPath = resource.UrlOrPath,
            UploaderName = $"{resource.Uploader.FirstName} {resource.Uploader.LastName}".Trim(),
            CreatedAt = resource.CreatedAt,
            IsPinned = resource.IsPinned
        };

        ViewBag.GroupId = resource.StudyGroupId;

        return View(viewModel);
    }

    // POST: /Resources/Delete/{id}[HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var resource = await _context.Resources
            .Include(r => r.StudyGroup)
                .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resource == null) return NotFound();

        var isPlatformAdmin = User.IsInRole("PlatformAdmin");
        var isAdmin = resource.StudyGroup.Members.Any(m => m.UserId == userId && m.Role == "Admin");
        var isUploader = resource.UploaderId == userId;

        // Security: Only Uploader, Group Admin, OR Platform Admin can delete
        if (!isAdmin && !isUploader && !isPlatformAdmin) return Forbid();

        var groupId = resource.StudyGroupId;

        if (resource.Type != "Link" && !string.IsNullOrWhiteSpace(resource.UrlOrPath))
        {
            var fileName = Path.GetFileName(resource.UrlOrPath);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", "resources", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.Resources.Remove(resource);

        var activity = new ActivityFeed
        {
            StudyGroupId = groupId,
            UserId = userId,
            Content = $"Deleted resource: {resource.Title}",
            Type = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityFeeds.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Groups", new { id = groupId });
    }
}