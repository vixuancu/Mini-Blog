using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBlogAPI.Models.DTOs;
using MiniBlogAPI.Models.Entities;
using MiniBlogAPI.Repositories;

namespace MiniBlogAPI.Controller;

[ApiController]
[Route("api/posts/{postId}/comments")] // Nested route: /api/posts/{postId}/comments
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILogger<CommentsController> logger)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _logger = logger;
    }
    // Lấy tất cả comments của 1 post.
    // GET /api/posts/{postId}/comments
    [HttpGet]
    [AllowAnonymous] // ai cũng xem được comments
    public async Task<IActionResult> GetCommentsByPostId(int postId)
    {
        try
        {
            // verify post exists
            var postExists = await _postRepository.ExistsAsync(postId);
            if (!postExists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Post not found"
                });
            }
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                PostId = c.PostId,
                Author = new CommentAuthorDto
                {
                    Id = c.User.Id,
                    Username = c.User.Username,
                    DisplayName = c.User.DisplayName,
                    ProfileImagePath = c.User.ProfileImagePath
                }
            });

            return Ok(new
            {
                success = true,
                postId = postId,
                count = commentDtos.Count(),
                data = commentDtos
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for postId {PostId}", postId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving comments"
            });
        }
    }
    // Lấy 1 comment cụ thể.
    // GET /api/posts/{postId}/comments/{commentId}
    [HttpGet("{commentId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommentById(int postId, int commentId)
    {
        try
        {
            var comment = await _commentRepository.GetWithDetailsAsync(commentId);
            if (comment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Comment not found "
                });
            }
            // verify comment thuộc về post này
            if (comment.PostId != postId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Comment does not belong to the specified post"
                });
            }

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                PostId = comment.PostId,
                Author = new CommentAuthorDto
                {
                    Id = comment.User.Id,
                    Username = comment.User.Username,
                    DisplayName = comment.User.DisplayName,
                    ProfileImagePath = comment.User.ProfileImagePath
                }
            };

            return Ok(new
            {
                success = true,
                data = commentDto
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commentId {CommentId} for postId {PostId}", commentId, postId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving the comment"
            });
        }
    }

    // Tạo comment mới cho post - YÊU CẦU AUTHENTICATION.
    // POST /api/posts/{postId}/comments
    [HttpPost]
    [Authorize]// phải login mới  comment được 
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDto createCommentDto)
    {
        try
        {
            // Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid token: User ID missing"
                });
            }
            // verify post exists
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Post not found"
                });
            }



            // verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            // Tạo comment
            var comment = new Comment
            {
                Content = createCommentDto.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _commentRepository.AddAsync(comment);
            _logger.LogInformation("✅Comment created by user {UserId} on post {PostId}", userId, postId);
            // Load chi tiết để trả về DTO
            var createdComment = await _commentRepository.GetWithDetailsAsync(comment.Id);
            var commentDto = new CommentDto
            {
                Id = createdComment!.Id,
                Content = createdComment.Content,
                CreatedAt = createdComment.CreatedAt,
                PostId = createdComment.PostId,
                Author = new CommentAuthorDto
                {
                    Id = createdComment.User.Id,
                    Username = createdComment.User.Username,
                    DisplayName = createdComment.User.DisplayName,
                    ProfileImagePath = createdComment.User.ProfileImagePath
                }
            };

            return CreatedAtAction(
                nameof(GetCommentById),
                new { postId = postId, commentId = comment.Id },
                new { success = true, message = "Comment created successfully", data = commentDto }
            );
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for postId {PostId}", postId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating the comment"
            });
        }
    }
    // Update comment - CHỈ CHỦ COMMENT MỚI UPDATE ĐƯỢC.
    // PUT /api/posts/{postId}/comments/{commentId}

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int postId, int commentId, [FromBody] CreateCommentDto updateCommentDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }
            // lấy comment hiện tại
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { success = false, message = "Comment not found" });
            }
            // Verify comment thuộc về post này
            if (comment.PostId != postId)
            {
                return BadRequest(new { success = false, message = "Comment does not belong to this post" });
            }
            // AUTHORIZATION CHECK: Chỉ chủ comment mới update được
            if (comment.UserId != userId)
            {
                return Forbid();  // HTTP 403 Forbidden
            }
            // update content
            comment.Content = updateCommentDto.Content;

            // save changes
            await _commentRepository.UpdateAsync(comment);
            _logger.LogInformation("Comment {CommentId} updated by user {UserId}", commentId, userId);

            // load updated comment details
            var updatedComment = await _commentRepository.GetWithDetailsAsync(comment.Id);

            var commentDto = new CommentDto
            {
                Id = updatedComment!.Id,
                Content = updatedComment.Content,
                CreatedAt = updatedComment.CreatedAt,
                PostId = updatedComment.PostId,
                Author = new CommentAuthorDto
                {
                    Id = updatedComment.User.Id,
                    Username = updatedComment.User.Username,
                    DisplayName = updatedComment.User.DisplayName,
                    ProfileImagePath = updatedComment.User.ProfileImagePath
                }
            };
            return Ok(new { success = true, message = "Comment updated successfully", data = commentDto });

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return StatusCode(500, new { success = false, message = "Error updating comment" });
        }
    }
    /// <summary>
    /// Delete comment - CHỈ CHỦ COMMENT MỚI XÓA ĐƯỢC.
    /// DELETE /api/posts/{postId}/comments/{commentId}
    /// </summary>
    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int postId, int commentId)
    {
        try
        {
            // Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            // Lấy comment hiện tại
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { success = false, message = "Comment not found" });
            }

            // Verify comment thuộc về post này
            if (comment.PostId != postId)
            {
                return BadRequest(new { success = false, message = "Comment does not belong to this post" });
            }

            // AUTHORIZATION CHECK: Chỉ chủ comment mới xóa được
            if (comment.UserId != userId)
            {
                return Forbid();  // HTTP 403 Forbidden
            }

            // Delete comment
            await _commentRepository.DeleteAsync(commentId);

            _logger.LogInformation("Comment {CommentId} deleted by user {UserId}", commentId, userId);

            return Ok(new { success = true, message = "Comment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return StatusCode(500, new { success = false, message = "Error deleting comment" });
        }
    }
    /// <summary>
    /// Lấy tất cả comments của current user (across all posts).
    /// GET /api/comments/my-comments
    /// </summary>
    [HttpGet("~/api/comments/my-comments")]  // Override route - không cần postId
    [Authorize]
    public async Task<IActionResult> GetMyComments()
    {
        try
        {
            // Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var comments = await _commentRepository.GetByUserIdAsync(userId);

            // Map to DTOs
            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                PostId = c.PostId,
                Author = new CommentAuthorDto
                {
                    Id = c.User.Id,
                    Username = c.User.Username,
                    DisplayName = c.User.DisplayName,
                    ProfileImagePath = c.User.ProfileImagePath
                }
            });

            return Ok(new
            {
                success = true,
                count = commentDtos.Count(),
                data = commentDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user comments");
            return StatusCode(500, new { success = false, message = "Error retrieving your comments" });
        }
    }
}
