using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

public class VerifyPhoneNumberValidator : AbstractValidator<VerifyPhoneNumberViewModel>
{
    public VerifyPhoneNumberValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits");
    }
}
