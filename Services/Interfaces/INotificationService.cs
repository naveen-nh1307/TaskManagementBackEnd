namespace TaskManagementSystem.Services.Interfaces;

public interface INotificationService
{
    Task CreateTaskAssignedNotificationAsync(int taskId, int userId, string taskTitle);
    Task CreateStatusChangedNotificationAsync(int taskId, int userId, string taskTitle, string oldStatus, string newStatus);
    Task<List<Models.DTOs.Tasks.CommentDto>> GetUserNotificationsAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}
