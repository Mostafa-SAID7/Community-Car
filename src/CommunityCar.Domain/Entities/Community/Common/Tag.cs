using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.Common;

public class Tag : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public new string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int UsageCount { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public virtual ICollection<QuestionTag> QuestionTags { get; private set; } = new List<QuestionTag>();

    private Tag() { }

    public Tag(string name, string slug, string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(slug, nameof(slug));

        Name = name;
        Slug = slug;
        Description = description;
        UsageCount = 0;
    }

    public void Update(string name, string slug, string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(slug, nameof(slug));

        Name = name;
        Slug = slug;
        Description = description;
    }

    public void IncrementUsage() => UsageCount++;
    
    public void DecrementUsage()
    {
        if (UsageCount > 0)
            UsageCount--;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
