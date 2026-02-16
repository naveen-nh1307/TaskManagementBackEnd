using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models.Entities;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationService(
        ApplicationDbContext context,
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task CreateTaskAssignedNotificationAsync(int taskId, int userId, string taskTitle)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskId = taskId,
                Type = "TaskAssigned",
                Message = $"You have been assigned a new task: {taskTitle}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send mock email
            await SendMockEmailAsync(userId, "New Task Assigned", notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task assigned notification");
        }
    }

    public async Task CreateStatusChangedNotificationAsync(int taskId, int userId, string taskTitle, string oldStatus, string newStatus)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskId = taskId,
                Type = "StatusChanged",
                Message = $"Task '{taskTitle}' status changed from {oldStatus} to {newStatus}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send mock email
            await SendMockEmailAsync(userId, "Task Status Updated", notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating status changed notification");
        }
    }

    public async Task<List<Models.DTOs.Tasks.CommentDto>> GetUserNotificationsAsync(int userId)
    {
        // This should return NotificationDto instead of CommentDto
        // For now, return empty list - will fix with proper NotificationDto
        return new List<Models.DTOs.Tasks.CommentDto>();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
        }
    }

    private async Task SendMockEmailAsync(int userId, string subject, string message)
    {
        var useMockEmail = _configuration.GetValue<bool>("EmailSettings:UseMockEmail", true);

        if (useMockEmail)
        {
            var user = await _context.Users.FindAsync(userId);
            _logger.LogInformation(
                "[MOCK EMAIL] To: {Email}, Subject: {Subject}, Message: {Message}",
                user?.Email ?? "unknown",
                subject,
                message);
        }
        else
        {
            // TODO: Implement real email sending via SMTP
            _logger.LogInformation("Real email sending not implemented yet");
        }
    }
}
