namespace MiniBlogAPI.Models;

// Model để bind JWT settings từ appsettings.json.
// Sử dụng Options Pattern của ASP.NET Core.
public class JwtSettings
{
    // Section name trong appsettings.json.
    public const string SectionName = "JwtSettings";


    // Secret key dùng để sign JWT token.\
    public string SecretKey {get; set ;} = string.Empty;

    // Issuer của token (định danh server issue token).
    // Claim: "iss"
    public string Issuer { get; set; } = string.Empty;

    // Audience của token (định danh client/app nhận token).
    // Claim: "aud"
    public string Audience { get; set; } = string.Empty;

    // Thời gian hết hạn token tính bằng phút.
    // Claim: "exp"
    public int ExpirationMinutes { get; set ;} = 60;
}