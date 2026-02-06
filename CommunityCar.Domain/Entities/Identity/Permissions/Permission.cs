using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Entities.Identity.Permissions;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Group { get; set; } = string.Empty;
}
