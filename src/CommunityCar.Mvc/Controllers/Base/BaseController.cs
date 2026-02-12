using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Base;

public abstract class BaseController : Controller
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");
    }

    protected string? GetCurrentUserName()
    {
        return User.Identity?.Name;
    }

    protected bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }
}
