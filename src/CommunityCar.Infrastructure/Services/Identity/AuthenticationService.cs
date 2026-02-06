using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Identity;

namespace CommunityCar.Infrastructure.Services.Identity;

/// <summary>
/// Service implementation for authentication operations
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<ApplicationUser>> LoginAsync(
        string email, 
        string password, 
        bool rememberMe, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
            return Result.Failure<ApplicationUser>("Invalid email or password");

        var result = await _signInManager.PasswordSignInAsync(
            user, 
            password, 
            rememberMe, 
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Result.Failure<ApplicationUser>("Account is locked out");
            
            if (result.RequiresTwoFactor)
                return Result.Failure<ApplicationUser>("Two-factor authentication required");
            
            return Result.Failure<ApplicationUser>("Invalid email or password");
        }

        return Result.Success(user);
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _signInManager.SignOutAsync();
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId, 
        string currentPassword, 
        string newPassword, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return Result.Failure("User not found");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to change password: {errors}");
        }

        return Result.Success();
    }
}
