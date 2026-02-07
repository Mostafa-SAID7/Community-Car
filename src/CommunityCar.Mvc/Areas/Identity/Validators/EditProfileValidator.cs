using FluentValidation;
using CommunityCar.Web.Areas.Identity.ViewModels;

namespace CommunityCar.Web.Areas.Identity.Validators;

public class EditProfileValidator : AbstractValidator<EditProfileViewModel>
{
    public EditProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Website)
            .Must(BeAValidUrl).WithMessage("Invalid website URL")
            .MaximumLength(200).WithMessage("Website URL cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Invalid date of birth")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
