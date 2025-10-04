namespace POS.Application.Common.Exceptions;

/// <summary>
/// Base exception class for all application exceptions
/// </summary>
public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected ApplicationException(string message, string errorCode, int statusCode) 
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected ApplicationException(string message, string errorCode, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
