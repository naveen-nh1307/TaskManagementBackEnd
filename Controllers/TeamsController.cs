using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models.DTOs.Teams;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTeams()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeamById(int id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);

        if (team == null)
        {
            return NotFound(new { message = "Team not found." });
        }

        return Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var team = await _teamService.CreateTeamAsync(dto);

        if (team == null)
        {
            return BadRequest(new { message = "Failed to create team. Manager may not exist or not have Manager role." });
        }

        return CreatedAtAction(nameof(GetTeamById), new { id = team.Id }, team);
    }

    [HttpPost("{id}/members")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] int userId)
    {
        var success = await _teamService.AddTeamMemberAsync(id, userId);

        if (!success)
        {
            return BadRequest(new { message = "Failed to add team member. Team or user may not exist, or user is already a member." });
        }

        return Ok(new { message = "Team member added successfully." });
    }

    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveTeamMember(int id, int userId)
    {
        var success = await _teamService.RemoveTeamMemberAsync(id, userId);

        if (!success)
        {
            return NotFound(new { message = "Team member not found." });
        }

        return NoContent();
    }
}
