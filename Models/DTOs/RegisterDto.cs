using System.ComponentModel.DataAnnotations;

namespace MiniBlogAPI.Models.DTOs;

/// DTO cho user registration request.
public class RegisterDto
{
    /// Username - unique.
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    /// Email - unique và valid format.
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// Password - sẽ được hash trước khi lưu.
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    /// Display name của user.
    [Required(ErrorMessage = "Display name is required")]
    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
    public string DisplayName { get; set; } = string.Empty;
}