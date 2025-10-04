using FluentValidation.Results;
using POS.Application.Common.Constants;

namespace POS.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() 
        : base(ErrorMessages.ValidationError, ErrorCodes.VAL_REQUIRED_FIELD, 400) // HTTP 400 Bad Request
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string message) 
        : base(message, ErrorCodes.VAL_INVALID_FORMAT, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
