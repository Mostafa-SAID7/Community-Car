using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Entities.Identity.Users;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CommunityCar.Web.Areas.Identity.ViewModels;
using CommunityCar.Domain.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Web.Areas.Identity.Controllers.Account;

[Area("Identity")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IEmailSender<ApplicationUser>? _emailSender;
    private readonly ISmsSender? _smsSender;
    private readonly ISmsService? _smsService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager, 
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IEmailSender<ApplicationUser>? emailSender = null,
        ISmsSender? smsSender = null,
        ISmsService? smsService = null)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _smsService = smsService;
    }

    #region Login

    [HttpGet]
    [AllowAnonymous]
    [Route("/Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("/Login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, 
            model.Password, 
            model.RememberMe, 
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account locked out.", model.Email);
            return RedirectToAction(nameof(Lockout));
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Email not confirmed. Please check your email.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    #endregion

    #region Register

    [HttpGet]
    [AllowAnonymous]
    [Route("/Register")]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("/Register")]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} created a new account with password.", model.Email);

            // Send confirmation email if email sender is configured
            if (_emailSender != null)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { area = "Identity", userId = user.Id, code },
                    protocol: Request.Scheme);

                // Note: Implement email sending logic
                _logger.LogInformation("Email confirmation link: {CallbackUrl}", callbackUrl);
            }

            // Auto sign-in after registration
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    #endregion

    #region Email Confirmation

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        
        ViewData["StatusMessage"] = result.Succeeded 
            ? "Thank you for confirming your email." 
            : "Error confirming your email.";

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResendEmailConfirmation()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return View(model);
        }

        if (_emailSender != null)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { area = "Identity", userId = user.Id, code },
                protocol: Request.Scheme);

            _logger.LogInformation("Resent email confirmation link: {CallbackUrl}", callbackUrl);
        }

        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        return View(model);
    }

    #endregion

    #region Two-Factor Authentication

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["RememberMe"] = rememberMe;

        return View(new LoginWith2faViewModel { RememberMe = rememberMe });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            authenticatorCode, 
            model.RememberMe, 
            model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }

        _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string? returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }

        _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        return View(model);
    }

    #endregion

    #region External Login

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { area = "Identity", returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["Error"] = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["Error"] = "Error loading external login information.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, 
            info.ProviderKey, 
            isPersistent: false, 
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Provider} provider.", info.LoginProvider);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToAction(nameof(Lockout));
        }

        // User doesn't have an account, create one
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Provider"] = info.LoginProvider;
        
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email ?? string.Empty });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["Error"] = "Error loading external login information during confirmation.";
            return RedirectToAction(nameof(Login));
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created an account using {Provider} provider.", info.LoginProvider);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    #endregion

    #region Password Reset

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action(
            nameof(ResetPassword),
            "Account",
            new { area = "Identity", code },
            protocol: Request.Scheme);

        if (_emailSender != null)
        {
            _logger.LogInformation("Password reset link: {CallbackUrl}", callbackUrl);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        return View(new ResetPasswordViewModel { Code = code });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} reset their password.", model.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    #endregion

    #region Change Password

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            TempData["Success"] = "Your password has been changed.";
            return RedirectToAction("Index", "Profiles", new { area = "Identity" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    #endregion

    #region Lockout

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }

    #endregion

    #region Logout

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/Logout")]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Feed", new { area = "" });
    }

    #endregion

    #region Access Denied

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion

    #region Phone Number Confirmation & OTP

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ConfirmPhoneNumber()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user.");
        }

        var model = new ConfirmPhoneNumberViewModel
        {
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendPhoneConfirmationCode(ConfirmPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("ConfirmPhoneNumber", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user.");
        }

        // Update phone number if changed
        if (user.PhoneNumber != model.PhoneNumber)
        {
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = false;
            await _userManager.UpdateAsync(user);
        }

        // Generate phone confirmation token
        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);

        if (_smsSender != null)
        {
            await _smsSender.SendSmsAsync(model.PhoneNumber, $"Your verification code is: {code}");
            _logger.LogInformation("Sent phone confirmation code to {PhoneNumber}", model.PhoneNumber);
        }
        else
        {
            // For development/testing - log the code
            _logger.LogWarning("SMS sender not configured. Verification code: {Code}", code);
            TempData["Info"] = $"SMS sender not configured. Your code is: {code}";
        }

        return RedirectToAction(nameof(VerifyPhoneNumber), new { phoneNumber = model.PhoneNumber });
    }

    [HttpGet]
    [Authorize]
    public IActionResult VerifyPhoneNumber(string phoneNumber)
    {
        var model = new VerifyPhoneNumberViewModel
        {
            PhoneNumber = phoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user.");
        }

        var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
        if (result.Succeeded)
        {
            _logger.LogInformation("User confirmed phone number {PhoneNumber}", model.PhoneNumber);
            TempData["Success"] = "Your phone number has been confirmed.";
            return RedirectToAction("Index", "Profiles", new { area = "Identity" });
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendPhoneConfirmationCode(string phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user.");
        }

        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        if (_smsSender != null)
        {
            await _smsSender.SendSmsAsync(phoneNumber, $"Your verification code is: {code}");
            _logger.LogInformation("Resent phone confirmation code to {PhoneNumber}", phoneNumber);
        }
        else
        {
            _logger.LogWarning("SMS sender not configured. Verification code: {Code}", code);
            TempData["Info"] = $"SMS sender not configured. Your code is: {code}";
        }

        TempData["Success"] = "Verification code has been resent.";
        return RedirectToAction(nameof(VerifyPhoneNumber), new { phoneNumber });
    }

    #endregion

    #region OTP Login

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoginWithOtp()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOtpCode(SendOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("LoginWithOtp", model);
        }

        var user = await _userManager.FindByNameAsync(model.PhoneNumber);
        if (user == null)
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
        }

        if (user == null || !user.PhoneNumberConfirmed)
        {
            // Don't reveal that the user doesn't exist or phone not confirmed
            return RedirectToAction(nameof(VerifyOtpCode), new { phoneNumber = model.PhoneNumber });
        }

        // Generate OTP token
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

        if (_smsSender != null)
        {
            await _smsSender.SendSmsAsync(model.PhoneNumber, $"Your login code is: {code}");
            _logger.LogInformation("Sent OTP code to {PhoneNumber}", model.PhoneNumber);
        }
        else
        {
            _logger.LogWarning("SMS sender not configured. OTP code: {Code}", code);
            TempData["Info"] = $"SMS sender not configured. Your code is: {code}";
        }

        return RedirectToAction(nameof(VerifyOtpCode), new { phoneNumber = model.PhoneNumber, returnUrl = model.ReturnUrl });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyOtpCode(string phoneNumber, string? returnUrl = null)
    {
        var model = new VerifyOtpViewModel
        {
            PhoneNumber = phoneNumber,
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtpCode(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.PhoneNumber);
        if (user == null)
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
        }

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code.");
            return View(model);
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Phone", model.Code);
        if (isValid)
        {
            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
            _logger.LogInformation("User {PhoneNumber} logged in with OTP", model.PhoneNumber);
            return RedirectToLocal(model.ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtpCode(string phoneNumber, string? returnUrl = null)
    {
        var user = await _userManager.FindByNameAsync(phoneNumber);
        if (user == null)
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        if (user != null && user.PhoneNumberConfirmed)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

            if (_smsSender != null)
            {
                await _smsSender.SendSmsAsync(phoneNumber, $"Your login code is: {code}");
                _logger.LogInformation("Resent OTP code to {PhoneNumber}", phoneNumber);
            }
            else
            {
                _logger.LogWarning("SMS sender not configured. OTP code: {Code}", code);
                TempData["Info"] = $"SMS sender not configured. Your code is: {code}";
            }
        }

        TempData["Success"] = "Verification code has been resent.";
        return RedirectToAction(nameof(VerifyOtpCode), new { phoneNumber, returnUrl });
    }

    #endregion

    #region Phone Authentication & OTP

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoginWithPhone(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendLoginOtp(string phoneNumber, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            ModelState.AddModelError(string.Empty, "Phone number is required");
            return View("LoginWithPhone");
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found with this phone number");
            return View("LoginWithPhone");
        }

        if (_smsService != null)
        {
            var code = _smsService.GenerateOtp();
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            
            // Store token temporarily (in production, use distributed cache)
            TempData[$"PhoneToken_{phoneNumber}"] = token;
            TempData[$"PhoneCode_{phoneNumber}"] = code;
            
            await _smsService.SendOtpAsync(phoneNumber, code);
            _logger.LogInformation("Login OTP sent to {PhoneNumber}", phoneNumber);
        }

        return View("VerifyLoginOtp", new LoginWithPhoneViewModel { PhoneNumber = phoneNumber });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyLoginOtp(LoginWithPhoneViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid phone number or code");
            return View(model);
        }

        // Verify OTP (in production, verify against stored token with expiration)
        var storedCode = TempData[$"PhoneCode_{model.PhoneNumber}"] as string;
        if (storedCode != model.Code)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code");
            return View(model);
        }

        await _signInManager.SignInAsync(user, model.RememberMe);
        _logger.LogInformation("User {PhoneNumber} logged in with phone OTP", model.PhoneNumber);
        
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterWithPhone(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterWithPhone(RegisterWithPhoneViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if phone number already exists
        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Phone number already registered");
            return View(model);
        }

        // Send OTP for verification
        if (_smsService != null)
        {
            var code = _smsService.GenerateOtp();
            TempData[$"RegisterCode_{model.PhoneNumber}"] = code;
            TempData[$"RegisterModel_{model.PhoneNumber}"] = System.Text.Json.JsonSerializer.Serialize(model);
            
            await _smsService.SendOtpAsync(model.PhoneNumber, code);
            _logger.LogInformation("Registration OTP sent to {PhoneNumber}", model.PhoneNumber);
        }

        return View("VerifyPhoneRegistration", new VerifyPhoneCodeViewModel { PhoneNumber = model.PhoneNumber });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhoneRegistration(VerifyPhoneCodeViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Verify OTP
        var storedCode = TempData[$"RegisterCode_{model.PhoneNumber}"] as string;
        if (storedCode != model.Code)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code");
            TempData.Keep($"RegisterCode_{model.PhoneNumber}");
            TempData.Keep($"RegisterModel_{model.PhoneNumber}");
            return View(model);
        }

        // Retrieve registration data
        var modelJson = TempData[$"RegisterModel_{model.PhoneNumber}"] as string;
        if (string.IsNullOrEmpty(modelJson))
        {
            ModelState.AddModelError(string.Empty, "Registration session expired. Please start again.");
            return RedirectToAction(nameof(RegisterWithPhone));
        }

        var registerModel = System.Text.Json.JsonSerializer.Deserialize<RegisterWithPhoneViewModel>(modelJson);
        if (registerModel == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid registration data");
            return RedirectToAction(nameof(RegisterWithPhone));
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = registerModel.PhoneNumber,
            PhoneNumber = registerModel.PhoneNumber,
            PhoneNumberConfirmed = true,
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName,
            Email = registerModel.Email
        };

        var result = await _userManager.CreateAsync(user, registerModel.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User registered with phone number {PhoneNumber}", registerModel.PhoneNumber);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult AddPhoneNumber()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhoneNumber(SendPhoneCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Check if phone number already exists
        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            ModelState.AddModelError(string.Empty, "Phone number already in use");
            return View(model);
        }

        if (_smsService != null)
        {
            var code = _smsService.GenerateOtp();
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            
            TempData[$"PhoneToken_{model.PhoneNumber}"] = token;
            TempData[$"PhoneCode_{model.PhoneNumber}"] = code;
            
            await _smsService.SendOtpAsync(model.PhoneNumber, code);
            _logger.LogInformation("Phone verification code sent to {PhoneNumber}", model.PhoneNumber);
        }

        return View("VerifyPhoneNumber", new VerifyPhoneCodeViewModel { PhoneNumber = model.PhoneNumber });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Verify OTP
        var storedCode = TempData[$"PhoneCode_{model.PhoneNumber}"] as string;
        var storedToken = TempData[$"PhoneToken_{model.PhoneNumber}"] as string;

        if (storedCode != model.Code)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code");
            TempData.Keep($"PhoneCode_{model.PhoneNumber}");
            TempData.Keep($"PhoneToken_{model.PhoneNumber}");
            return View(model);
        }

        var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, storedToken ?? string.Empty);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {UserId} verified phone number {PhoneNumber}", user.Id, model.PhoneNumber);
            TempData["Success"] = "Phone number verified successfully";
            return RedirectToAction("Index", "Profiles", new { area = "Identity" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoneNumber()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.SetPhoneNumberAsync(user, null);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {UserId} removed phone number", user.Id);
            TempData["Success"] = "Phone number removed successfully";
        }
        else
        {
            TempData["Error"] = "Failed to remove phone number";
        }

        return RedirectToAction("Index", "Profiles", new { area = "Identity" });
    }

    #endregion

    #region Helper Methods

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Feed", new { area = "" });
    }

    #endregion
}
