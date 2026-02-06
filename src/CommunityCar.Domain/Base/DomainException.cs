namespace CommunityCar.Domain.Base;

public class DomainException : Exception
{
    public string BusinessMessage { get; }

    public DomainException(string businessMessage) : base(businessMessage)
    {
        BusinessMessage = businessMessage;
    }
}
