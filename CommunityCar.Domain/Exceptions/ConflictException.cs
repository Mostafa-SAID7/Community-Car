namespace CommunityCar.Domain.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ConflictException() 
        : base("The request conflicts with the current state of the resource.")
    {
    }
}
