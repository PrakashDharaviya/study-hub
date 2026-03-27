using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Resources;

namespace StudyHub.Web.Controllers;

/// <summary>
/// Manages the uploading of files and sharing of links within Study Groups.
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

    // GET: /Resources/Add/{groupId}
    [HttpGet]
    public async Task<IActionResult> Add(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // Security Check: Ensure the user is actually a member of this group
        var isMember = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == id && m.UserId == userId);

        if (!isMember)
        {
            // If they aren't a member, redirect them to the group details so they can join
            return RedirectToAction("Details", "Groups", new { id = id });
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

        // Custom Validation: Ensure they provided the correct data based on the type
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

        // Security Check: Double-check membership on POST
        var isMember = await _context.GroupMembers
            .AnyAsync(m => m.StudyGroupId == model.StudyGroupId && m.UserId == userId);

        if (!isMember) return Unauthorized();

        string finalUrlOrPath = string.Empty;

        // Handle File Upload
        if (model.ResourceType != "Link" && model.UploadedFile != null)
        {
            // 1. Define the upload folder path
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "resources");

            // 2. Create the directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. Generate a unique filename to prevent overwrites
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.UploadedFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Save the file physically to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.UploadedFile.CopyToAsync(fileStream);
            }

            // 5. Store the relative path in the database
            finalUrlOrPath = "/uploads/resources/" + uniqueFileName;
        }
        else
        {
            // Handle External Link
            finalUrlOrPath = model.Url!;
        }

        // Create the Resource Entity
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
        await _context.SaveChangesAsync();

        // Redirect back to the Group Details page, specifically to the resources tab
        return RedirectToAction("Details", "Groups", new { id = model.StudyGroupId });
    }
}