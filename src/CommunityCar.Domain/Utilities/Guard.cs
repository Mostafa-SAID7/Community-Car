using System;

namespace CommunityCar.Domain.Utilities;

/// <summary>
/// Simple Guard utility for domain-level validations.
/// </summary>
public interface IGuardClause { }

public class Guard : IGuardClause
{
    public static IGuardClause Against { get; } = new Guard();

    private Guard() { }
}

public static class GuardClauseExtensions
{
    public static void Null(this IGuardClause guardClause, object? value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void NullOrWhiteSpace(this IGuardClause guardClause, string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        }
    }

    public static void Empty(this IGuardClause guardClause, Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
        }
    }

    public static void Negative(this IGuardClause guardClause, decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{parameterName} cannot be negative.", parameterName);
        }
    }

    public static void Negative(this IGuardClause guardClause, double value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{parameterName} cannot be negative.", parameterName);
        }
    }
}
