using Microsoft.EntityFrameworkCore ;
using MiniBlogAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// ==================== Add services to the container ====================

// 1. Configure DbContext với PostgreSQL
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
// 2. Add Controllers support
builder.Services.AddControllers();

// 3. Add API Explorer (cho Swagger/OpenAPI)
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

// Map Controllers (API endpoints) bật routing để controller nhận request
app.MapControllers();

// ==================== Run the application ====================
app.Run();

