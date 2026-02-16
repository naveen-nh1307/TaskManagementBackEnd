using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models.DTOs.Tasks;

public class UpdateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
