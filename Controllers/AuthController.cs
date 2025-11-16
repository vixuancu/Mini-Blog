
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBlogAPI.Models.DTOs;
using MiniBlogAPI.Services;

namespace MiniBlogAPI.Controller ;

// Controller cho authentication operations (Register, Login).
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // POST /api/auth/register
    // <param name="registerDto">User registration data</param>
    // <returns>AuthResponse với JWT token</returns>
    [HttpPost("register")]
    [AllowAnonymous] // cho phep anonymous access (khong can JWT token)
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var response = await _authService.RegisterAsync(registerDto);

            return Ok(new
            {
                success = true,
                message = "User registered successfully",
                data = response
            }
            );
        }
        catch (InvalidOperationException ex)
        {
            // Username hoac email da ton tai
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during registration"
            });
        }
    }
    //POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous] // cho phep anonymous access (khong can JWT token)
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = response 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Invalid username or password
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during login"
            });
            
        }
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize] // Yêu cầu JWT token hợp lệ
    public IActionResult GetCurrentUser()
    {
       // Http.Context.user được set bởi UseAuthentication() middleware
       var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
       var username = User.Identity?.Name;
       var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
       return Ok(new
       {
           success = true,
           message = "Authenticated user ",
           data = new
           {
               Id = userId,
               Username = username,
               Email = email,
               claims = User.Claims.Select(c => new { c.Type, c.Value })
           }
       });
    }
}