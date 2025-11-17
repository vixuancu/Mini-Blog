namespace MiniBlogAPI.Exceptions;

/// Exception khi user không có quyền (HTTP 403).
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}