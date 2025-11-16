using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniBlogAPI.Models;
using MiniBlogAPI.Models.DTOs;
using MiniBlogAPI.Models.Entities;
using MiniBlogAPI.Repositories;

namespace MiniBlogAPI.Services ;

// Service implementation cho authentication operations.
public class  AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    // Constructor - inject dependencies.
    public AuthService(
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value; // Extract value từ IOptions
        _logger = logger;
    }
    // Register user mới.
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // 1 validate username chua ton tai
        if (await _userRepository.UsernameExistAsync(registerDto.Username))
        {
            throw new InvalidOperationException("username a;ready exists");
        }
        // 2. Validate email chưa tồn tại
        if (await _userRepository.EmailExistAsync(registerDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }
        // 3. Hash password
        var passwordHash = HashPassword(registerDto.Password);
        // 4. Tạo user entity
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
        // 5. Save vào database
        await _userRepository.AddAsync(user);
        _logger.LogInformation("✅New user registered: {Username}", user.Username);

        // 6. Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        // 7. Return response với token
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfileImagePath = user.ProfileImagePath,
                CreatedAt = user.CreatedAt
            }
        };
    }
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // 1. Tìm user theo username
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

        // 2. Validate user tồn tại
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // 3. Verify password
        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", loginDto.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        _logger.LogInformation("User logged in: {Username}", user.Username);

        // 4. Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        // 5. Return response với token
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfileImagePath = user.ProfileImagePath,
                CreatedAt = user.CreatedAt
            }
        };
    }


    // Generate JWT token với user claims.
    public string GenerateJwtToken(int userId, string username, string email)
    {
        // 1. Create claims (payload data)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),      // Subject (user ID)
            new Claim(JwtRegisteredClaimNames.UniqueName, username),        // Username
            new Claim(JwtRegisteredClaimNames.Email, email),                // Email
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (unique)
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued At
        };

        // 2. Create signing credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Create token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        // 4. Serialize token to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Hash password bằng BCrypt.
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    // Verify password với hash.
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}