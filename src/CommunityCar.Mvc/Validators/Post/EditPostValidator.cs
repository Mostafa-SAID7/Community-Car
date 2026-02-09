using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Mvc.ViewModels.Post;

namespace CommunityCar.Web.Validators.Post;

public class EditPostValidator : AbstractValidator<EditPostViewModel>
{
    private const long MaxImageSize = 10 * 1024 * 1024; // 10 MB
    private const long MaxVideoSize = 50 * 1024 * 1024; // 50 MB
    
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi" };

    public EditPostValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .MaximumLength(10000).WithMessage("Content cannot exceed 10,000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid post type");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid post status");

        // Image file validation
        RuleFor(x => x.ImageFile)
            .Must(BeValidImageSize).WithMessage($"Image file size cannot exceed {MaxImageSize / 1024 / 1024} MB")
            .Must(BeValidImageExtension).WithMessage($"Only {string.Join(", ", AllowedImageExtensions)} image formats are allowed")
            .When(x => x.ImageFile != null);

        // Video file validation
        RuleFor(x => x.VideoFile)
            .Must(BeValidVideoSize).WithMessage($"Video file size cannot exceed {MaxVideoSize / 1024 / 1024} MB")
            .Must(BeValidVideoExtension).WithMessage($"Only {string.Join(", ", AllowedVideoExtensions)} video formats are allowed")
            .When(x => x.VideoFile != null);

        // Link URL validation
        RuleFor(x => x.LinkUrl)
            .Must(BeValidUrl).WithMessage("Please enter a valid URL")
            .When(x => !string.IsNullOrEmpty(x.LinkUrl));

        // Link title validation when URL is provided
        RuleFor(x => x.LinkTitle)
            .NotEmpty().WithMessage("Link title is required when providing a URL")
            .MaximumLength(200).WithMessage("Link title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.LinkUrl));

        // Tags validation
        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Tags));
    }

    private bool BeValidImageSize(IFormFile? file)
    {
        if (file == null) return true;
        return file.Length <= MaxImageSize;
    }

    private bool BeValidImageExtension(IFormFile? file)
    {
        if (file == null) return true;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(extension);
    }

    private bool BeValidVideoSize(IFormFile? file)
    {
        if (file == null) return true;
        return file.Length <= MaxVideoSize;
    }

    private bool BeValidVideoExtension(IFormFile? file)
    {
        if (file == null) return true;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedVideoExtensions.Contains(extension);
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
