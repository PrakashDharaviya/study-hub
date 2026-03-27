using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Models.Notifications;
using StudyHub.Web.Services.Interfaces;

namespace StudyHub.Web.Services;

/// <summary>
/// Concrete implementation of INotificationService. Handles all EF Core database
/// operations for user notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly StudyHubDbContext _context;

    public NotificationService(StudyHubDbContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(string userId, string message, string type, string? actionUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<NotificationSummaryViewModel> GetNotificationSummaryAsync(string userId)
    {
        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        var recentNotifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new NotificationSummaryViewModel
        {
            UnreadCount = unreadCount,
            RecentNotifications = recentNotifications
        };
    }

    public async Task<IEnumerable<NotificationViewModel>> GetUserNotificationsAsync(string userId, bool? onlyUnread = null)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (onlyUnread.HasValue && onlyUnread.Value)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null || notification.IsRead)
        {
            return false;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}