using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class LoginWith2faValidator : AbstractValidator<LoginWith2faViewModel>
{
    public LoginWith2faValidator()
    {
        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithMessage("Authenticator code is required")
            .Length(6, 7).WithMessage("Authenticator code must be 6 or 7 characters");
    }
}
