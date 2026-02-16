using TaskManagementSystem.Models.DTOs.Tasks;

namespace TaskManagementSystem.Services.Interfaces;

public interface ITaskService
{
    Task<TaskDto?> CreateTaskAsync(CreateTaskDto dto, int createdById);
    Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId, string userRole);
    Task<List<TaskDto>> GetAllTasksAsync(int userId, string userRole);
    Task<List<TaskDto>> GetMyTasksAsync(int userId);
    Task<TaskDto?> UpdateTaskStatusAsync(int taskId, string newStatus, int userId, string userRole);
    Task<bool> DeleteTaskAsync(int taskId, int userId, string userRole);
    Task<CommentDto?> AddCommentAsync(int taskId, AddCommentDto dto, int userId);
    Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId, string userRole);
    Task<bool> DeleteCommentAsync(int commentId, int userId, string userRole);
    Task<List<CommentDto>> GetTaskCommentsAsync(int taskId);
}
