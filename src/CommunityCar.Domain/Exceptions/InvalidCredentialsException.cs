namespace CommunityCar.Domain.Exceptions;

/// <summary>
/// Exception for invalid credentials scenarios (401)
/// </summary>
public class InvalidCredentialsException : UnauthorizedException
{
    public InvalidCredentialsException() 
        : base("The provided credentials are invalid.")
    {
    }

    public InvalidCredentialsException(string message) : base(message)
    {
    }
}
