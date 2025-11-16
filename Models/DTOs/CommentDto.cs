namespace MiniBlogAPI.Models.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }

    // User info của người tạo comment.
    public CommentAuthorDto Author { get; set; } = null!;
}

/// DTO cho author info trong comment.
public class CommentAuthorDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
}