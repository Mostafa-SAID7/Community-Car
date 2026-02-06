using CommunityCar.Web.ViewModels.Community.Qa;
using FluentValidation;

namespace CommunityCar.Web.Validators.Qa;

public class CreateAnswerValidator : AbstractValidator<CreateAnswerViewModel>
{
    public CreateAnswerValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(20, 5000).WithMessage("Content must be between 20 and 5000 characters");
    }
}
