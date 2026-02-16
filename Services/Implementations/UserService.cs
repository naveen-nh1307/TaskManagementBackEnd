using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models.DTOs.Users;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<List<UserDto>> GetManagersAsync()
    {
        var managers = await _context.Users
            .Where(u => u.IsActive && (u.Role == "Manager" || u.Role == "Admin"))
            .OrderBy(u => u.FirstName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return managers;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
    {
        try
        {
            if (newRole != "Admin" && newRole != "Manager" && newRole != "User")
            {
                _logger.LogWarning("Invalid role: {Role}", newRole);
                return false;
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null || !user.IsActive)
            {
                return false;
            }

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role");
            return false;
        }
    }
}
