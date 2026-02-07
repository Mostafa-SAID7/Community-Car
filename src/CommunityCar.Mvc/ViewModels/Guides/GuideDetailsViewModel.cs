using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Guides;

public class GuideDetailsViewModel
{
    public GuideDto Guide { get; set; } = null!;
    public List<GuideStepDto> Steps { get; set; } = new();
    public PagedResult<GuideCommentDto> Comments { get; set; } = null!;
}
