using System.ComponentModel.DataAnnotations;

namespace MiniBlogAPI.Models.DTOs;

public class UpdatePostDto
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10000 characters")]
    public string? Content { get; set; }

    [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
    public string? ImagePath { get; set; }
}