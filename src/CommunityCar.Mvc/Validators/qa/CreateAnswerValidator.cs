using CommunityCar.Mvc.ViewModels.Qa;
using FluentValidation;

namespace CommunityCar.Mvc.Validators.Qa;

public class CreateAnswerValidator : AbstractValidator<CreateAnswerViewModel>
{
    public CreateAnswerValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(20, 5000).WithMessage("Content must be between 20 and 5000 characters")
            .Must(BeValidContent).WithMessage("Answer must contain meaningful content");
    }

    private bool BeValidContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        // Check if content has at least some letters or numbers
        return content.Count(char.IsLetterOrDigit) >= 10;
    }
}
