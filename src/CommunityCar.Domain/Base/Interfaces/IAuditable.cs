namespace CommunityCar.Domain.Base.Interfaces;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? ModifiedAt { get; set; }
    string? ModifiedBy { get; set; }
}
