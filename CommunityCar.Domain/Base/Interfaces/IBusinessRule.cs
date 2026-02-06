namespace CommunityCar.Domain.Base.Interfaces;

public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}
