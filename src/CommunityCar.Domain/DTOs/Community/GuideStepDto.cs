namespace CommunityCar.Domain.DTOs.Community;

public class GuideStepDto
{
    public Guid Id { get; set; }
    public Guid GuideId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public int EstimatedTimeMinutes { get; set; }
}
