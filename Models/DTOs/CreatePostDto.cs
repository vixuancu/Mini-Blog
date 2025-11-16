using System.ComponentModel.DataAnnotations;

namespace MiniBlogAPI.Models.DTOs;

public class CreatePostDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength =3, ErrorMessage ="Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(10000, MinimumLength =10, ErrorMessage ="Content must be between 10 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage ="ImagePath cannot exceed 500 characters")]
    public string? ImagePath { get; set; }
}