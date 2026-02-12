using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Identity.Roles;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Management;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Administration.Management;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Administration/Management/Users")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            usersQuery = usersQuery.Where(u => 
                u.Email.ToLower().Contains(searchTerm) || 
                (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchTerm))
            );
            ViewData["CurrentFilter"] = searchTerm;
        }

        var users = usersQuery.ToList();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "Unknown",
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roles,
                IsActive = !user.IsDeleted, // Assuming IsDeleted flag
                CreatedOn = user.CreatedAt
            });
        }

        return View(userViewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateUserViewModel
        {
            AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        // Check if user exists (including soft-deleted) to prevent Duplicate Key Exception
        // Standardize input for comparison
        var normalizedEmail = model.Email.ToUpper();
        
        var existingUser = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedEmail);

        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
            {
                ModelState.AddModelError(string.Empty, $"User with email '{model.Email}' already exists but is deleted. Please restore it instead.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"User with email '{model.Email}' already exists.");
            }
            model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.IsDeleted = true; // Soft delete
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Index)); // Or show error
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            CurrentRoles = userRoles,
            AllRoles = allRoles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        // Email update logic if needed

        await _userManager.UpdateAsync(user);

        // Update Roles
        var userRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = model.SelectedRoles.Except(userRoles);
        var rolesToRemove = userRoles.Except(model.SelectedRoles);

        await _userManager.AddToRolesAsync(user, rolesToAdd);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Settings() => View();
}


