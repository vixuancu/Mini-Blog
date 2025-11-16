namespace MiniBlogAPI.Models.DTOs;

/// DTO cho user info (không chứa sensitive data như password).
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}