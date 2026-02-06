using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Identity;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Infrastructure.Services.Identity;

/// <summary>
/// Service implementation for user operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<Result<ApplicationUser>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            return Result.Failure<ApplicationUser>("User not found");

        return Result.Success(user);
    }

    public async Task<Result<ApplicationUser>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        
        if (user == null)
            return Result.Failure<ApplicationUser>("User not found");

        return Result.Success(user);
    }

    public async Task<Result<PagedResult<ApplicationUser>>> SearchUsersAsync(
        QueryParameters parameters, 
        CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetPagedAsync(parameters, cancellationToken);
        return Result.Success(result);
    }

    public async Task<Result<Guid>> RegisterUserAsync(
        string email, 
        string password, 
        string firstName, 
        string lastName, 
        string? phoneNumber, 
        CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            return Result.Failure<Guid>("Email is already registered");

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Slug = SlugHelper.GenerateSlug($"{firstName} {lastName}")
        };

        var result = await _userManager.CreateAsync(user, password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<Guid>($"Failed to create user: {errors}");
        }

        return Result.Success(user.Id);
    }

    public async Task<Result> UpdateProfileAsync(
        Guid userId, 
        string firstName, 
        string lastName, 
        string? bio, 
        string? phoneNumber, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            return Result.Failure("User not found");

        user.UpdateProfile(firstName, lastName, bio ?? string.Empty);
        user.PhoneNumber = phoneNumber;

        await _userRepository.UpdateAsync(user, cancellationToken);
        
        return Result.Success();
    }
}
