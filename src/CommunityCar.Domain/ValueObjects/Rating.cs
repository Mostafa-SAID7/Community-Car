namespace CommunityCar.Domain.ValueObjects;

/// <summary>
/// Value Object representing a rating with half-star precision
/// Enforces business rules: 0-5 range with 0.5 increments
/// </summary>
public sealed class Rating : IEquatable<Rating>, IComparable<Rating>
{
    public const decimal Min = 0m;
    public const decimal Max = 5m;
    public const decimal Step = 0.5m;

    public decimal Value { get; }

    private Rating(decimal value)
    {
        Value = value;
    }

    public static Rating Create(decimal value)
    {
        if (value < Min || value > Max)
            throw new ArgumentException($"Rating must be between {Min} and {Max}", nameof(value));

        // Validate step increment (must be multiple of 0.5)
        if ((value * 2) % 1 != 0)
            throw new ArgumentException($"Rating must be in increments of {Step}", nameof(value));

        return new Rating(value);
    }

    public static Rating FromStars(int fullStars, bool hasHalfStar = false)
    {
        if (fullStars < 0 || fullStars > 5)
            throw new ArgumentException("Full stars must be between 0 and 5", nameof(fullStars));

        var value = fullStars + (hasHalfStar ? 0.5m : 0m);
        return Create(value);
    }

    public static Rating Zero => new(0m);
    public static Rating Half => new(0.5m);
    public static Rating One => new(1m);
    public static Rating OneAndHalf => new(1.5m);
    public static Rating Two => new(2m);
    public static Rating TwoAndHalf => new(2.5m);
    public static Rating Three => new(3m);
    public static Rating ThreeAndHalf => new(3.5m);
    public static Rating Four => new(4m);
    public static Rating FourAndHalf => new(4.5m);
    public static Rating Five => new(5m);

    public int FullStars => (int)Math.Floor(Value);
    public bool HasHalfStar => Value % 1 == 0.5m;
    public int TotalHalfStars => (int)(Value * 2);

    public bool Equals(Rating? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Rating other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public int CompareTo(Rating? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator ==(Rating? left, Rating? right) => 
        left is null ? right is null : left.Equals(right);
    public static bool operator !=(Rating? left, Rating? right) => !(left == right);
    public static bool operator <(Rating left, Rating right) => left.CompareTo(right) < 0;
    public static bool operator >(Rating left, Rating right) => left.CompareTo(right) > 0;
    public static bool operator <=(Rating left, Rating right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Rating left, Rating right) => left.CompareTo(right) >= 0;

    public override string ToString() => Value.ToString("F1");

    // Implicit conversion for EF Core
    public static implicit operator decimal(Rating rating) => rating.Value;
}
