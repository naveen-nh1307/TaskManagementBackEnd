using TaskManagementSystem.Models.DTOs.Users;

namespace TaskManagementSystem.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<List<UserDto>> GetManagersAsync();
    Task<bool> UpdateUserRoleAsync(int userId, string newRole);
}
