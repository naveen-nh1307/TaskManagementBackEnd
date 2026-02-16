using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Helpers;
using TaskManagementSystem.Models.DTOs.Auth;
using TaskManagementSystem.Models.Entities;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        JwtTokenHelper jwtTokenHelper,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtTokenHelper = jwtTokenHelper;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", registerDto.Email);
                return null;
            }

            // Validate role
            if (registerDto.Role != "Admin" && registerDto.Role != "Manager" && registerDto.Role != "User")
            {
                _logger.LogWarning("Invalid role: {Role}", registerDto.Role);
                return null;
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtTokenHelper.GenerateToken(user.Id, user.Email, user.Role);
            var expiresAt = DateTime.UtcNow.AddMinutes(1440);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return null;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found or inactive - {Email}", loginDto.Email);
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", loginDto.Email);
                return null;
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtTokenHelper.GenerateToken(user.Id, user.Email, user.Role);
            var expiresAt = DateTime.UtcNow.AddMinutes(1440);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return null;
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => new UserInfoDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role
            })
            .FirstOrDefaultAsync();

        return user;
    }
}
