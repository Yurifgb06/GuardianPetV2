using System.Linq.Expressions;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories;

// Composes EF expressions only; never materializes or constructs SQL from input.
public static class CollectionQuery
{
    public static IQueryable<T> Apply<T, TKey>(IQueryable<T> query, CollectionQueryDTO options,
        Expression<Func<T, TKey>> key, Expression<Func<T, long>> id)
    {
        options.EnsureValid();
        if (options.SortBy is not null || options.SortDirection is not null || options.Page.HasValue || options.PageSize.HasValue)
        {
            var ordered = string.Equals(options.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(key) : query.OrderBy(key);
            query = ordered.ThenBy(id);
        }
        if (options.Page.HasValue || options.PageSize.HasValue)
            query = query.Skip(checked(((options.Page ?? 1) - 1) * (options.PageSize ?? 20))).Take(options.PageSize ?? 20);
        return query;
    }
}
