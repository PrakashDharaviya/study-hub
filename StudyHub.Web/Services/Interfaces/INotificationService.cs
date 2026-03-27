using StudyHub.Web.Models.Notifications;

namespace StudyHub.Web.Services.Interfaces;

/// <summary>
/// Defines the contract for managing user notifications and alerts.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates and saves a new notification for a specific user.
    /// </summary>
    Task CreateNotificationAsync(string userId, string message, string type, string? actionUrl = null);

    /// <summary>
    /// Retrieves the unread count and the most recent notifications for the navbar bell.
    /// </summary>
    Task<NotificationSummaryViewModel> GetNotificationSummaryAsync(string userId);

    /// <summary>
    /// Retrieves all notifications for a user, optionally filtered by read status.
    /// </summary>
    Task<IEnumerable<NotificationViewModel>> GetUserNotificationsAsync(string userId, bool? onlyUnread = null);

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, string userId);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    Task MarkAllAsReadAsync(string userId);
}