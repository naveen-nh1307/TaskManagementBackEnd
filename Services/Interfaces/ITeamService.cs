using TaskManagementSystem.Models.DTOs.Teams;

namespace TaskManagementSystem.Services.Interfaces;

public interface ITeamService
{
    Task<TeamDto?> CreateTeamAsync(CreateTeamDto dto);
    Task<TeamDto?> GetTeamByIdAsync(int teamId);
    Task<List<TeamDto>> GetAllTeamsAsync();
    Task<bool> AddTeamMemberAsync(int teamId, int userId);
    Task<bool> RemoveTeamMemberAsync(int teamId, int userId);
}
