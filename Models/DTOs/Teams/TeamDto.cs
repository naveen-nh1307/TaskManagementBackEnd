namespace TaskManagementSystem.Models.DTOs.Teams;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<TeamMemberDto> Members { get; set; } = new();
    public int TaskCount { get; set; }
}

public class TeamMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
