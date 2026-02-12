using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordViewModel>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");
    }
}
