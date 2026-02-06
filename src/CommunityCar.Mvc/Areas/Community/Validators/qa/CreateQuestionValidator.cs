using CommunityCar.Web.Areas.Community.ViewModels.qa;
using FluentValidation;

namespace CommunityCar.Web.Areas.Community.Validators.qa;

public class CreateQuestionValidator : AbstractValidator<CreateQuestionViewModel>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(10, 200).WithMessage("Title must be between 10 and 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(20, 5000).WithMessage("Content must be between 20 and 5000 characters");

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");
    }
}
