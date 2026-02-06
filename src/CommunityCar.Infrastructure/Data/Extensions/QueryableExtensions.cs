using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CommunityCar.Domain.Base;

namespace CommunityCar.Infrastructure.Data.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public static async Task<PagedResult<T>> ApplyQueryParametersAsync<T>(
        this IQueryable<T> query, 
        QueryParameters parameters, 
        CancellationToken cancellationToken = default)
    {
        query = query.ApplySorting(parameters.SortBy, parameters.SortDescending);
        
        return await query.ToPagedResultAsync(parameters.PageNumber, parameters.PageSize, cancellationToken);
    }
}
