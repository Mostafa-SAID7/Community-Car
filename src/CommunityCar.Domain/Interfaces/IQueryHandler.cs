using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Interfaces;

/// <summary>
/// Interface for query handlers
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
