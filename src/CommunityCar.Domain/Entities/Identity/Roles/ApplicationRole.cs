using System;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CommunityCar.Domain.Entities.Identity.Roles;

public class ApplicationRole : IdentityRole<Guid>, IEntity
{
    public string? Description { get; set; }
}
