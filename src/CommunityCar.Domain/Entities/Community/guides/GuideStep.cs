using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.guides;

public class GuideStep : BaseEntity
{
    public Guid GuideId { get; private set; }
    public virtual Guide Guide { get; private set; } = null!;
    
    public int StepNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public string? VideoUrl { get; private set; }
    public int EstimatedTimeMinutes { get; private set; }

    private GuideStep() { }

    public GuideStep(Guid guideId, int stepNumber, string title, string content, int estimatedTimeMinutes)
    {
        Guard.Against.Empty(guideId, nameof(guideId));
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        GuideId = guideId;
        StepNumber = stepNumber;
        Title = title;
        Content = content;
        EstimatedTimeMinutes = estimatedTimeMinutes;
    }

    public void Update(string title, string content, int estimatedTimeMinutes)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        Title = title;
        Content = content;
        EstimatedTimeMinutes = estimatedTimeMinutes;
    }

    public void SetMedia(string? imageUrl, string? videoUrl)
    {
        ImageUrl = imageUrl;
        VideoUrl = videoUrl;
    }
}
