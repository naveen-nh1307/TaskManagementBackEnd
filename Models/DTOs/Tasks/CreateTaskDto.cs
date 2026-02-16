using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models.DTOs.Tasks;

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    public int AssignedToId { get; set; }

    public int? TeamId { get; set; }
}
