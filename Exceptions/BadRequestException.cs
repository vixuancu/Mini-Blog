namespace MiniBlogAPI.Exceptions;

/// Exception khi request không hợp lệ (HTTP 400).
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}