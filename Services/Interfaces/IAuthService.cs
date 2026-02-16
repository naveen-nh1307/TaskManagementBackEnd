using TaskManagementSystem.Models.DTOs.Auth;

namespace TaskManagementSystem.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<UserInfoDto?> GetCurrentUserAsync(int userId);
}
