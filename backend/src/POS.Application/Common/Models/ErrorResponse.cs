namespace POS.Application.Common.Models;

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Default constructor
    public ErrorResponse()
    {
    }

    // Constructor with error code and message
    public ErrorResponse(string errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }

    // Constructor with error code, message, and errors dictionary
    public ErrorResponse(string errorCode, string message, IDictionary<string, string[]> errors)
    {
        ErrorCode = errorCode;
        Message = message;
        Errors = errors;
        Timestamp = DateTime.UtcNow;
    }
}
