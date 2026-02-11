using CommunityCar.Mvc.ViewModels.Reviews;
using CommunityCar.Domain.ValueObjects;
using FluentValidation;

namespace CommunityCar.Mvc.Validators.Review;

public class CreateReviewValidator : AbstractValidator<CreateReviewViewModel>
{
    public CreateReviewValidator()
    {
        // EntityId and EntityType are now optional for standalone reviews
        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityType))
            .WithMessage("Entity type cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid review type");

        RuleFor(x => x.Rating)
            .Must(BeValidRating)
            .WithMessage($"Rating must be between 1 and {Rating.Max} in increments of {Rating.Step}");

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
        
        // Image validation
        RuleFor(x => x.Images)
            .Must(images => images == null || images.Count <= 5)
            .WithMessage("You can upload maximum 5 images")
            .Must(images => images == null || images.All(img => img.Length <= 5 * 1024 * 1024))
            .WithMessage("Each image must be less than 5MB");
    }

    private bool BeValidRating(decimal rating)
    {
        // Rating must be between 1 and 5
        if (rating < 1 || rating > 5)
            return false;
            
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
