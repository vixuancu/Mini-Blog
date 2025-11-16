using MiniBlogAPI.Models.DTOs;

namespace MiniBlogAPI.Services;

/// Service interface cho authentication operations.
public interface IAuthService
{
    /// Register user mới.
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// Login user và generate JWT token.
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

    /// Generate JWT token cho user.
    string GenerateJwtToken(int userId, string username, string email);

    /// Hash password bằng BCrypt.
    string HashPassword(string password);

    /// Verify password với hash.
    bool VerifyPassword(string password, string passwordHash);
}