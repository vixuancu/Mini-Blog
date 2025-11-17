namespace MiniBlogAPI.Models;

/// Standard error response format cho API.
public class ErrorResponse
{
    /// Luôn false cho error responses.
    public bool Success { get; set; } = false;

    /// HTTP status code.
    public int StatusCode { get; set; }

    /// Error message cho user.
    public string Message { get; set; } = string.Empty;

    /// Chi tiết lỗi (chỉ hiển thị trong Development).
    public string? Details { get; set; }

    /// Trace ID để tracking logs.
    public string? TraceId { get; set; }

    /// Timestamp khi lỗi xảy ra.
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// Validation errors (cho model validation failures).
    public Dictionary<string, string[]>? Errors { get; set; }
}