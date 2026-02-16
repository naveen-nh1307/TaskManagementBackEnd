using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementSystem.Models.DTOs.Tasks;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var tasks = await _taskService.GetAllTasksAsync(userId.Value, userRole);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var task = await _taskService.GetTaskByIdAsync(id, userId.Value, userRole);

        if (task == null)
        {
            return NotFound(new { message = "Task not found or access denied." });
        }

        return Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var task = await _taskService.CreateTaskAsync(dto, userId.Value);

        if (task == null)
        {
            return BadRequest(new { message = "Failed to create task. Assigned user may not exist." });
        }

        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var task = await _taskService.UpdateTaskStatusAsync(id, dto.Status, userId.Value, userRole);

        if (task == null)
        {
            return NotFound(new { message = "Task not found or access denied." });
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var success = await _taskService.DeleteTaskAsync(id, userId.Value, userRole);

        if (!success)
        {
            return NotFound(new { message = "Task not found or access denied." });
        }

        return NoContent();
    }

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var tasks = await _taskService.GetMyTasksAsync(userId.Value);
        return Ok(tasks);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var comment = await _taskService.AddCommentAsync(id, dto, userId.Value);

        if (comment == null)
        {
            return NotFound(new { message = "Task not found." });
        }

        return Ok(comment);
    }

    [HttpPut("comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var comment = await _taskService.UpdateCommentAsync(commentId, dto, userId.Value, userRole);

        if (comment == null)
        {
            return NotFound(new { message = "Comment not found or access denied." });
        }

        return Ok(comment);
    }

    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userId == null || userRole == null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var success = await _taskService.DeleteCommentAsync(commentId, userId.Value, userRole);

        if (!success)
        {
            return NotFound(new { message = "Comment not found or access denied." });
        }

        return NoContent();
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetTaskComments(int id)
    {
        var comments = await _taskService.GetTaskCommentsAsync(id);
        return Ok(comments);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }
}
