using CommunityCar.Web.Areas.Community.ViewModels.qa;
using FluentValidation;

namespace CommunityCar.Web.Areas.Community.Validators.qa;

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
