using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models.DTOs.Tasks;

public class AddCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}

public class CommentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
