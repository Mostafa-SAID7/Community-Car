using CommunityCar.Domain.Entities.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Web.Areas.Identity.Controllers.Users;

[Area("Identity")]
[Route("Identity/[controller]")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        var currentUserId = _userManager.GetUserId(User);
        var query = _userManager.Users.Where(u => u.Id.ToString() != currentUserId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.UserName!.Contains(searchTerm) || 
                                   u.Email!.Contains(searchTerm) || 
                                   u.FirstName!.Contains(searchTerm) || 
                                   u.LastName!.Contains(searchTerm));
        }

        var users = await query.Take(20).ToListAsync();
        ViewBag.SearchTerm = searchTerm;
        return View(users);
    }
}
