using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models.DTOs.Teams;
using TaskManagementSystem.Models.Entities;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeamService> _logger;

    public TeamService(ApplicationDbContext context, ILogger<TeamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TeamDto?> CreateTeamAsync(CreateTeamDto dto)
    {
        try
        {
            // Validate manager exists and has Manager or Admin role
            var manager = await _context.Users.FindAsync(dto.ManagerId);
            if (manager == null || !manager.IsActive || (manager.Role != "Manager" && manager.Role != "Admin"))
            {
                _logger.LogWarning("Manager {ManagerId} not found or not valid", dto.ManagerId);
                return null;
            }

            var team = new Team
            {
                Name = dto.Name,
                Description = dto.Description,
                ManagerId = dto.ManagerId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Add team members
            foreach (var memberId in dto.MemberIds)
            {
                var user = await _context.Users.FindAsync(memberId);
                if (user != null && user.IsActive)
                {
                    var teamMember = new TeamMember
                    {
                        TeamId = team.Id,
                        UserId = memberId,
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.TeamMembers.Add(teamMember);
                }
            }

            await _context.SaveChangesAsync();

            return await GetTeamByIdAsync(team.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return null;
        }
    }

    public async Task<TeamDto?> GetTeamByIdAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            return null;
        }

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            ManagerId = team.ManagerId,
            ManagerName = $"{team.Manager.FirstName} {team.Manager.LastName}",
            CreatedAt = team.CreatedAt,
            IsActive = team.IsActive,
            Members = team.TeamMembers.Select(tm => new TeamMemberDto
            {
                Id = tm.Id,
                UserId = tm.UserId,
                UserName = $"{tm.User.FirstName} {tm.User.LastName}",
                Email = tm.User.Email,
                JoinedAt = tm.JoinedAt
            }).ToList(),
            TaskCount = team.Tasks.Count
        };
    }

    public async Task<List<TeamDto>> GetAllTeamsAsync()
    {
        var teams = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Tasks)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return teams.Select(team => new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            ManagerId = team.ManagerId,
            ManagerName = $"{team.Manager.FirstName} {team.Manager.LastName}",
            CreatedAt = team.CreatedAt,
            IsActive = team.IsActive,
            Members = team.TeamMembers.Select(tm => new TeamMemberDto
            {
                Id = tm.Id,
                UserId = tm.UserId,
                UserName = $"{tm.User.FirstName} {tm.User.LastName}",
                Email = tm.User.Email,
                JoinedAt = tm.JoinedAt
            }).ToList(),
            TaskCount = team.Tasks.Count
        }).ToList();
    }

    public async Task<bool> AddTeamMemberAsync(int teamId, int userId)
    {
        try
        {
            var team = await _context.Teams.FindAsync(teamId);
            var user = await _context.Users.FindAsync(userId);

            if (team == null || !team.IsActive || user == null || !user.IsActive)
            {
                return false;
            }

            // Check if already a member
            var existing = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (existing != null)
            {
                _logger.LogWarning("User {UserId} is already a member of team {TeamId}", userId, teamId);
                return false;
            }

            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team member");
            return false;
        }
    }

    public async Task<bool> RemoveTeamMemberAsync(int teamId, int userId)
    {
        try
        {
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (teamMember == null)
            {
                return false;
            }

            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team member");
            return false;
        }
    }
}
