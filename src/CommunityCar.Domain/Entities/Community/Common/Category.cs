using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.Common;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public new string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    
    public virtual ICollection<Question> Questions { get; private set; } = new List<Question>();

    private Category() { }

    public Category(string name, string slug, string? description = null, string? icon = null, string? color = null, int displayOrder = 0)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(slug, nameof(slug));

        Name = name;
        Slug = slug;
        Description = description;
        Icon = icon;
        Color = color;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void Update(string name, string slug, string? description = null, string? icon = null, string? color = null, int? displayOrder = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(slug, nameof(slug));

        Name = name;
        Slug = slug;
        Description = description;
        Icon = icon;
        Color = color;
        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
