using CommunityCar.Mvc.ViewModels.Reviews;
using CommunityCar.Domain.ValueObjects;
using FluentValidation;

namespace CommunityCar.Mvc.Validators.Review;

public class CreateReviewValidator : AbstractValidator<CreateReviewViewModel>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("Entity ID is required");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .MaximumLength(100)
            .WithMessage("Entity type cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid review type");

        RuleFor(x => x.Rating)
            .Must(BeValidRating)
            .WithMessage($"Rating must be between {Rating.Min} and {Rating.Max} in increments of {Rating.Step}");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .MinimumLength(5)
            .WithMessage("Title must be at least 5 characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Review content is required")
            .MinimumLength(50)
            .WithMessage("Review must be at least 50 characters")
            .MaximumLength(5000)
            .WithMessage("Review cannot exceed 5000 characters");

        RuleFor(x => x.Pros)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Pros))
            .WithMessage("Pros cannot exceed 1000 characters");

        RuleFor(x => x.Cons)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Cons))
            .WithMessage("Cons cannot exceed 1000 characters");
    }

    private bool BeValidRating(decimal rating)
    {
        try
        {
            Rating.Create(rating);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
