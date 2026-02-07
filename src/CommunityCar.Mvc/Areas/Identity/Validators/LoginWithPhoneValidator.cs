using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

public class LoginWithPhoneValidator : AbstractValidator<LoginWithPhoneViewModel>
{
    public LoginWithPhoneValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP must contain only digits");
    }
}
