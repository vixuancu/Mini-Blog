using System.ComponentModel.DataAnnotations;

namespace MiniBlogAPI.Models.DTOs ;

// DTO cho login request
public class LoginDto
{
    // Username hoặc Email để login.
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}