using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniBlogAPI.Data;
using MiniBlogAPI.Models;
using MiniBlogAPI.Repositories;
using MiniBlogAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== Add services to the container ====================

//  Configure DbContext với PostgreSQL
builder.Services.AddDbContext<BlogContext>(options =>
{
    // Lấy connection string từ appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Sử dụng PostgreSQL provider
    options.UseNpgsql(connectionString);

    // Enable sensitive data logging trong Development (để debug)
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

//Configure JWT Settings (Options Pattern)
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName)
);

//Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    // Set JWT Bearer làm default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure JWT Bearer options
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate Issuer (người issue token)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings?.Issuer,

        // Validate Audience (người nhận token)
        ValidateAudience = true,
        ValidAudience = jwtSettings?.Audience,

        // Validate chữ ký (signature)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey is not configured"))
        ),

        // Validate token expiration
        ValidateLifetime = true,

        // Clock skew = cho phép sai lệch thời gian giữa server (default 5 phút)
        ClockSkew = TimeSpan.Zero  // Set = 0 để token expire chính xác
    };
    // Event handlers (optional - cho debugging)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Log khi authentication fails
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { error = "You are not authorized" });
            return context.Response.WriteAsync(result);
        }
    };
});


//  Register Repositories với Dependency Injection
// Scoped lifetime = 1 instance per HTTP request
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();            // User-specific
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>(); 

// 5. Register Services với DI
builder.Services.AddScoped<IAuthService, AuthService>();

//  Add Controllers support
builder.Services.AddControllers();

//  Add API Explorer (cho Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ==================== Build the app ====================
var app = builder.Build();

// ==================== Configure the HTTP request pipeline ====================

// Development: Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// redirect http to https
app.UseHttpsRedirection();

// Serve static files từ wwwroot (cho images)
app.UseStaticFiles();

// 1. Routing - phải đầu tiên để xác định endpoint
app.UseRouting();

// 2. Authentication - verify JWT token, set User claims
app.UseAuthentication();

// 3. Authorization - check [Authorize] attributes
app.UseAuthorization();

// Map Controllers (API endpoints) bật routing để controller nhận request
app.MapControllers();

// ==================== Run the application ====================
app.Run();

