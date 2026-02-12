using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class VerifyPhoneCodeValidator : AbstractValidator<VerifyPhoneCodeViewModel>
{
    public VerifyPhoneCodeValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits");
    }
}
