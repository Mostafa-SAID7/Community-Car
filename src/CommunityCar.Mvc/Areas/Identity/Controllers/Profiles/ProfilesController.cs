using CommunityCar.Domain.Entities.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Identity.Controllers.Profiles;

[Area("Identity")]
[Route("Identity/[controller]")]
public class ProfilesController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfilesController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? userId)
    {
        if (userId == null)
        {
            // Fallback to current user or error
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                return RedirectToAction(nameof(Index), new { userId = currentUser.Id });
            }
            return NotFound("User ID is required.");
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return View(user);
    }
}
