using FluentValidation;
using POS.Application.DTOs.Auth;

namespace POS.Application.Validators.Auth;

public class PinLoginRequestValidator : AbstractValidator<PinLoginRequestDto>
{
    public PinLoginRequestValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("PIN is required")
            .Matches(@"^\d{4}$").WithMessage("PIN must be exactly 4 digits");

        RuleFor(x => x.StoreId)
            .GreaterThanOrEqualTo(0).WithMessage("Store ID must be 0 or greater");
            // StoreId = 0 for customers, > 0 for staff
    }
}
