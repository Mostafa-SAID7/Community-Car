using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

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
