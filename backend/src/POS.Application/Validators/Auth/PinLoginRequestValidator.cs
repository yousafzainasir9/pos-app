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
            .GreaterThan(0).WithMessage("Valid Store ID is required");
    }
}
