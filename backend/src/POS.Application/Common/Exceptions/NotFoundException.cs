using POS.Application.Common.Constants;

namespace POS.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) 
        : base(message, ErrorCodes.RES_NOT_FOUND, 404) // HTTP 404 Not Found
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found", ErrorCodes.RES_NOT_FOUND, 404)
    {
    }
}
