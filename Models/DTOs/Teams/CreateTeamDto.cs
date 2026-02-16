using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models.DTOs.Teams;

public class CreateTeamDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int ManagerId { get; set; }

    public List<int> MemberIds { get; set; } = new();
}
