using CommunityCar.Mvc.ViewModels.Qa;
using FluentValidation;

namespace CommunityCar.Mvc.Validators.Qa;

public class CreateQuestionValidator : AbstractValidator<CreateQuestionViewModel>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(10, 200).WithMessage("Title must be between 10 and 200 characters")
            .Must(BeAValidTitle).WithMessage("Title must not contain only special characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(20, 5000).WithMessage("Content must be between 20 and 5000 characters");

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters")
            .Must(BeValidTags).WithMessage("Tags must be comma-separated words");
    }

    private bool BeAValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        return title.Any(char.IsLetterOrDigit);
    }

    private bool BeValidTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return true;

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return tagList.All(tag => !string.IsNullOrWhiteSpace(tag.Trim()));
    }
}
