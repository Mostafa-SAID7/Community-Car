using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordViewModel>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        // Password validation is handled by PasswordRequirementsAttribute and JavaScript
        // FluentValidation regex patterns removed to prevent conflicts with ASP.NET Identity
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters")
            .MaximumLength(100).WithMessage("New password cannot exceed 100 characters")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
