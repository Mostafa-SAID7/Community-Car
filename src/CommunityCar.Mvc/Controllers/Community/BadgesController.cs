using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Community;

public class BadgesController : BaseController
{
    private readonly ILogger<BadgesController> _logger;

    public BadgesController(ILogger<BadgesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        // TODO: Implement badge service integration
        // For now, the view has static badge data
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult MyBadges()
    {
        try
        {
            var userId = GetCurrentUserId();
            // TODO: Implement user badge retrieval
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user badges");
            return View();
        }
    }
}
