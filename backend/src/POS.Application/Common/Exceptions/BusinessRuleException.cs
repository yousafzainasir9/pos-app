using POS.Application.Common.Constants;

namespace POS.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : ApplicationException
{
    public BusinessRuleException(string message, string errorCode) 
        : base(message, errorCode, 422) // HTTP 422 Unprocessable Entity
    {
    }

    public static BusinessRuleException InsufficientStock(string productName) =>
        new BusinessRuleException(
            $"Insufficient stock for product: {productName}", 
            ErrorCodes.BIZ_INSUFFICIENT_STOCK);

    public static BusinessRuleException ShiftAlreadyOpen(string userName) =>
        new BusinessRuleException(
            $"User {userName} already has an open shift", 
            ErrorCodes.BIZ_SHIFT_ALREADY_OPEN);

    public static BusinessRuleException NoActiveShift() =>
        new BusinessRuleException(
            "No active shift found. Please start a shift first", 
            ErrorCodes.BIZ_NO_ACTIVE_SHIFT);

    public static BusinessRuleException InvalidPaymentAmount(decimal required, decimal provided) =>
        new BusinessRuleException(
            $"Payment amount ${provided} is less than required ${required}", 
            ErrorCodes.BIZ_INVALID_PAYMENT_AMOUNT);
}
