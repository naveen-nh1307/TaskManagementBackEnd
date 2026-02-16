using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models.DTOs.Tasks;
using TaskManagementSystem.Models.Entities;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<TaskService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto dto, int createdById)
    {
        try
        {
            // Validate assigned user exists
            var assignedUser = await _context.Users.FindAsync(dto.AssignedToId);
            if (assignedUser == null || !assignedUser.IsActive)
            {
                _logger.LogWarning("Assigned user {UserId} not found or inactive", dto.AssignedToId);
                return null;
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Deadline = dto.Deadline,
                AssignedToId = dto.AssignedToId,
                CreatedById = createdById,
                TeamId = dto.TeamId,
                Status = "ToDo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Create notification for assigned user
            await _notificationService.CreateTaskAssignedNotificationAsync(task.Id, task.AssignedToId, task.Title);

            return await GetTaskDtoAsync(task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return null;
        }
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId, string userRole)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return null;
        }

        // Role-based access control
        if (userRole == "User" && task.AssignedToId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access task {TaskId} without permission", userId, taskId);
            return null;
        }

        return MapToTaskDto(task);
    }

    public async Task<List<TaskDto>> GetAllTasksAsync(int userId, string userRole)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Team);

        // Filter based on role
        if (userRole == "User")
        {
            query = query.Where(t => t.AssignedToId == userId);
        }
        else if (userRole == "Manager")
        {
            // Managers see tasks they created or tasks in their teams
            var managedTeamIds = await _context.Teams
                .Where(team => team.ManagerId == userId)
                .Select(team => team.Id)
                .ToListAsync();

            query = query.Where(t => t.CreatedById == userId || 
                                    (t.TeamId.HasValue && managedTeamIds.Contains(t.TeamId.Value)));
        }
        // Admins see all tasks (no filter)

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return tasks.Select(MapToTaskDto).ToList();
    }

    public async Task<List<TaskDto>> GetMyTasksAsync(int userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Team)
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToTaskDto).ToList();
    }

    public async Task<TaskDto?> UpdateTaskStatusAsync(int taskId, string newStatus, int userId, string userRole)
    {
        try
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return null;
            }

            // Access control: Users can only update their own tasks
            if (userRole == "User" && task.AssignedToId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update task {TaskId} without permission", userId, taskId);
                return null;
            }

            var oldStatus = task.Status;
            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification if status changed
            if (oldStatus != newStatus && task.CreatedById != userId)
            {
                await _notificationService.CreateStatusChangedNotificationAsync(
                    task.Id, task.CreatedById, task.Title, oldStatus, newStatus);
            }

            return await GetTaskDtoAsync(taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status");
            return null;
        }
    }

    public async Task<bool> DeleteTaskAsync(int taskId, int userId, string userRole)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
            {
                return false;
            }

            // Only Admins and Managers can delete tasks
            if (userRole == "User")
            {
                _logger.LogWarning("User {UserId} attempted to delete task {TaskId} without permission", userId, taskId);
                return false;
            }

            // Managers can only delete tasks they created
            if (userRole == "Manager" && task.CreatedById != userId)
            {
                _logger.LogWarning("Manager {UserId} attempted to delete task {TaskId} created by someone else", userId, taskId);
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return false;
        }
    }

    public async Task<CommentDto?> AddCommentAsync(int taskId, AddCommentDto dto, int userId)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return null;
            }

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with user information
            var savedComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return savedComment == null ? null : new CommentDto
            {
                Id = savedComment.Id,
                TaskId = savedComment.TaskId,
                UserId = savedComment.UserId,
                UserName = $"{savedComment.User.FirstName} {savedComment.User.LastName}",
                Content = savedComment.Content,
                CreatedAt = savedComment.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return null;
        }
    }

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId, string userRole)
    {
        try
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return null;
            }

            // Only author can update comment
            if (comment.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update comment {CommentId} without permission", userId, commentId);
                return null;
            }

            comment.Content = dto.Content;
            // comment.UpdatedAt = DateTime.UtcNow; // No UpdatedAt field in model

            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = $"{comment.User.FirstName} {comment.User.LastName}",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment");
            return null;
        }
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int userId, string userRole)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return false;
            }

            // Author, Admin, and Manager can delete comments
            if (comment.UserId != userId && userRole != "Admin" && userRole != "Manager")
            {
                _logger.LogWarning("User {UserId} attempted to delete comment {CommentId} without permission", userId, commentId);
                return false;
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            return false;
        }
    }

    public async Task<List<CommentDto>> GetTaskCommentsAsync(int taskId)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return comments;
    }

    private async Task<TaskDto?> GetTaskDtoAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Team)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        return task == null ? null : MapToTaskDto(task);
    }

    private TaskDto MapToTaskDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Deadline = task.Deadline,
            AssignedToId = task.AssignedToId,
            AssignedToName = $"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}",
            CreatedById = task.CreatedById,
            CreatedByName = $"{task.CreatedBy.FirstName} {task.CreatedBy.LastName}",
            TeamId = task.TeamId,
            TeamName = task.Team?.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CommentCount = task.Comments?.Count ?? 0
        };
    }
}
