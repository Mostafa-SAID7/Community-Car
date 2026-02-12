using FluentValidation;
using CommunityCar.Mvc.Areas.Identity.ViewModels;

namespace CommunityCar.Mvc.Areas.Identity.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset code is required");
    }
}
