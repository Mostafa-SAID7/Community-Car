using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Base;

public abstract class BaseEntity : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Slug { get; set; }

    // Concurrency
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Auditing
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    protected BaseEntity() { }
}
