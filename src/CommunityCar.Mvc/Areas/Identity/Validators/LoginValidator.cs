using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
