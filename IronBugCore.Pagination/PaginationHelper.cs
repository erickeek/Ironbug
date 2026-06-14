namespace IronbugCore.Pagination;

public static class PaginationHelper
{
    public static PagedQuery<T> Paginate<T>(this IQueryable<T> query, QueryFilter filter)
    {
        return new PagedQuery<T>(query, filter);
    }

    public static async Task<PagedQuery<T>> PaginateAsync<T>(this IQueryable<T> query, QueryFilter filter, CancellationToken cancellationToken = default)
    {
        var pagedQuery = new PagedQuery<T>(query, filter);
        await pagedQuery.GetTotalAsync(cancellationToken);

        return pagedQuery;
    }
}