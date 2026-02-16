using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models.DTOs.Tasks;

public class UpdateTaskStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // ToDo, InProgress, Done
}
