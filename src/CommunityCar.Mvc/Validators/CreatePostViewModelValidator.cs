using CommunityCar.Mvc.ViewModels.Post;
using FluentValidation;

namespace CommunityCar.Web.Validators;

public class CreatePostViewModelValidator : AbstractValidator<CreatePostViewModel>
{
    public CreatePostViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

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
            .Must(BeValidImageSize).WithMessage("Image file size cannot exceed 10 MB")
            .When(x => x.ImageFile != null);

        RuleFor(x => x.ImageFile)
            .Must(BeValidImageType).WithMessage("Only JPG, JPEG, PNG, GIF, WEBP, and BMP images are allowed")
            .When(x => x.ImageFile != null);

        // Video file validation
        RuleFor(x => x.VideoFile)
            .Must(BeValidVideoSize).WithMessage("Video file size cannot exceed 50 MB")
            .When(x => x.VideoFile != null);

        RuleFor(x => x.VideoFile)
            .Must(BeValidVideoType).WithMessage("Only MP4, WEBM, OGG, MOV, and AVI videos are allowed")
            .When(x => x.VideoFile != null);

        // Link URL validation
        RuleFor(x => x.LinkUrl)
            .Must(BeValidUrl).WithMessage("Please enter a valid URL")
            .When(x => !string.IsNullOrEmpty(x.LinkUrl));
    }

    private bool BeValidImageSize(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true;
        const long maxSize = 10 * 1024 * 1024; // 10 MB
        return file.Length <= maxSize;
    }

    private bool BeValidImageType(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true;
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    private bool BeValidVideoSize(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true;
        const long maxSize = 50 * 1024 * 1024; // 50 MB
        return file.Length <= maxSize;
    }

    private bool BeValidVideoType(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true;
        var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi" };
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
