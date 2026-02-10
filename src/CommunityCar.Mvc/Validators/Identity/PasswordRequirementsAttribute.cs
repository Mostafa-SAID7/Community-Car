using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CommunityCar.Web.Validators.Identity;

/// <summary>
/// Validates password meets the application's password requirements
/// </summary>
public class PasswordRequirementsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success; // Let [Required] handle empty values
        }

        var password = value.ToString()!;

        // Check minimum length
        if (password.Length < 6)
        {
            return new ValidationResult("Password must be at least 6 characters long");
        }

        // Check for uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return new ValidationResult("Password must contain at least one uppercase letter");
        }

        // Check for lowercase letter
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return new ValidationResult("Password must contain at least one lowercase letter");
        }

        // Check for digit
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return new ValidationResult("Password must contain at least one digit");
        }

        return ValidationResult.Success;
    }
}
