using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class TwoFactorValidator : AbstractValidator<TwoFactorViewModel>
{
    public TwoFactorValidator()
    {
        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithMessage("Authenticator code is required")
            .Length(6, 7).WithMessage("Authenticator code must be 6-7 characters")
            .Matches(@"^\d+$").WithMessage("Authenticator code must contain only digits");
    }
}
