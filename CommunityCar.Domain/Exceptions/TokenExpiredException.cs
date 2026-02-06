namespace CommunityCar.Domain.Exceptions;

/// <summary>
/// Exception for expired token scenarios (401)
/// </summary>
public class TokenExpiredException : UnauthorizedException
{
    public TokenExpiredException() 
        : base("The authentication token has expired.")
    {
    }

    public TokenExpiredException(string message) : base(message)
    {
    }
}
