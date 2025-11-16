namespace MiniBlogAPI.Models.DTOs;

/// DTO cho authentication response (login/register success).\
public class AuthResponseDto
{
    /// JWT access token.
    public string Token { get; set; } = string.Empty;

    /// Token expiration time (UTC).
    public DateTime ExpiresAt { get; set; }

    /// User info.
    public UserDto User { get; set; } = null!;
}