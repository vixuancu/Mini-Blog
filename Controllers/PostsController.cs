using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBlogAPI.Exceptions;
using MiniBlogAPI.Models.DTOs;
using MiniBlogAPI.Models.Entities;
using MiniBlogAPI.Repositories;

namespace MiniBlogAPI.Controller ;

// controllers cho Posts (CURD với authorization).
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILogger<PostsController> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    // Lấy tất cả posts với pagination.
    // GET /api/posts?pageNumber=1&pageSize=10
    [HttpGet]
    [AllowAnonymous] // public endpoint - không cần JWT token
    public async Task<IActionResult> GetAllPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // validate pagination
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var posts = await _postRepository.GetPagedAsync(pageNumber, pageSize);
            var totalCount = await _postRepository.CountAsync();

            // map entities to DTOs 
            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    DisplayName = p.User.DisplayName,
                    ProfileImagePath = p.User.ProfileImagePath
                },
                CommentCount = p.Comments?.Count ?? 0

            });
            return Ok(new
            {
                success = true,
                data = postDtos,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error fetching posts");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching posts"
            });
        }
    }
    // Lấy post theo ID với full details (user, comments).
    // GET /api/posts/{id}
    [HttpGet("{id}")]
    [AllowAnonymous] // public endpoint - không cần JWT token
    public async Task<IActionResult> GetPostById(int id)
    {
            var post = await _postRepository.GetWithDetailsAsync(id);
            if (post == null)
            {
                throw new NotFoundException("Post", id);  // Middleware catch & return 404
            }

            // map entity to DTO
            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImagePath = post.ImagePath,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = post.User.Id,
                    Username = post.User.Username,
                    DisplayName = post.User.DisplayName,
                    ProfileImagePath = post.User.ProfileImagePath
                },
                CommentCount = post.Comments?.Count ?? 0
            };

            return Ok(new { success = true, data = post });
            // Không cần try-catch - middleware sẽ handle!
        
    }

    // Search posts theo title.
    // GET /api/posts/search?query=keyword 
    [HttpGet("search")]
    [AllowAnonymous] // public endpoint - không cần JWT token
    public async Task<IActionResult> SearchPosts([FromQuery] string query)
    {
        try
        {   
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Query parameter is required"
                });
            }
            var posts = await _postRepository.SearchByTitleAsync(query);

            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    DisplayName = p.User.DisplayName,
                    ProfileImagePath = p.User.ProfileImagePath
                },
                CommentCount = p.Comments?.Count ?? 0
            });

            return Ok(new
            {
                success = true,
                query = query,
                count = postDtos.Count(),
                data = postDtos
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error searching posts");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while searching posts"
            });
        }
    }

    // Tạo post mới - YÊU CẦU AUTHENTICATION.
    // POST /api/posts 
    [HttpPost]
    [Authorize] // phai có JWT token
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
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

            // Tạo post - SIMPLE, không cần service
            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                ImagePath = createPostDto.ImagePath,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _postRepository.AddAsync(post);

            _logger.LogInformation("Post created by user {UserId}: {PostTitle}", userId, post.Title);
            // load user info de return DTO
            var createdPost = await _postRepository.GetWithDetailsAsync(post.Id);

            var postDto = new PostDto
            {
                Id = createdPost!.Id,
                Title = createdPost.Title,
                Content = createdPost.Content,
                ImagePath = createdPost.ImagePath,
                CreatedAt = createdPost.CreatedAt,
                UpdatedAt = createdPost.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = createdPost.User.Id,
                    Username = createdPost.User.Username,
                    DisplayName = createdPost.User.DisplayName,
                    ProfileImagePath = createdPost.User.ProfileImagePath
                },
                CommentCount = 0
            };
            return CreatedAtAction(
                nameof(GetPostById),
                 new { id = postDto.Id }, 
                 new {
                    success = true,
                    data = postDto
                 });
        }  
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating the post"
            });
        }
    }
    // Update post - CHỈ CHỦ POST MỚI UPDATE ĐƯỢC.
    // PUT /api/posts/{id}

    [HttpPut("{id}")]
    [Authorize] // phai có JWT token
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
    {
        try
        {
            // lấy user từ JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid token: User ID missing"
                });
            }
            // lấy post hiện tại
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Post not found"
                });
            }
            // AUTHORIZATION CHECK: Chỉ chủ post mới update được
            if (post.UserId != userId)
            {
                return Forbid();// HTTP 403 Forbidden
            }
            // Update fields nếu có trong DTO
            if (!string.IsNullOrEmpty(updatePostDto.Title))
            {
                post.Title = updatePostDto.Title;
            }
            if (!string.IsNullOrWhiteSpace(updatePostDto.Content))
            {
                post.Content = updatePostDto.Content;
            }

            if (updatePostDto.ImagePath != null)
            {
                post.ImagePath = updatePostDto.ImagePath;
            }
            post.UpdatedAt = DateTime.UtcNow;

            //save changes
            await _postRepository.UpdateAsync(post);
            _logger.LogInformation("Post {PostId} updated by user {UserId}", post.Id, userId);

            // Load updated post với details
            var updatedPost = await _postRepository.GetWithDetailsAsync(id);
            var postDto = new PostDto
            {
                Id = updatedPost!.Id,
                Title = updatedPost.Title,
                Content = updatedPost.Content,
                ImagePath = updatedPost.ImagePath,
                CreatedAt = updatedPost.CreatedAt,
                UpdatedAt = updatedPost.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = updatedPost.User.Id,
                    Username = updatedPost.User.Username,
                    DisplayName = updatedPost.User.DisplayName,
                    ProfileImagePath = updatedPost.User.ProfileImagePath
                },
                CommentCount = updatedPost.Comments?.Count ?? 0
            };
            return Ok(new
            {
                success = true,
                message = "Post updated successfully",
                data = postDto
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Error updating post" });
        }
    }

    // Delete post - CHỈ CHỦ POST MỚI DELETE ĐƯỢC.
    // DELETE /api/posts/{id}
    [HttpDelete("{id}")]
    [Authorize] // phai có JWT token
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            // lấy user từ JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid token: User ID missing"
                });
            }
            // lấy post hiện tại
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Post not found"
                });
            }
            // AUTHORIZATION CHECK: Chỉ chủ post mới xóa được
            if (post.UserId != userId)
            {
                return Forbid();// HTTP 403 Forbidden
            }
            // Xóa post
            await _postRepository.DeleteAsync(post);
            _logger.LogInformation("Post {PostId} deleted by user {UserId}", post.Id, userId);
            return Ok(new
            {
                success = true,
                message = "Post deleted successfully"
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Error deleting post" });
        }
    }
    // Lấy tất cả posts của current user.
    // GET /api/posts/my-posts
    [HttpGet("my-posts")]
    [Authorize] // phai có JWT token
    public async Task<IActionResult> GetMyPosts()
    {
        try
        {
            // lấy user từ JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid token: User ID missing"
                });
            }
            var posts = await _postRepository.GetByUserIdAsync(userId);
            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Author = new PostAuthorDto
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    DisplayName = p.User.DisplayName,
                    ProfileImagePath = p.User.ProfileImagePath
                },
                CommentCount = p.Comments?.Count ?? 0
            });
            return Ok(new
            {
                success = true,
                count = postDtos.Count(),
                data = postDtos
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error fetching user's posts");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching your posts"
            });
        }
    }
}