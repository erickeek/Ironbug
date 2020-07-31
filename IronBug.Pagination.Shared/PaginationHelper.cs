using System;
using System.Linq;

namespace IronBug.Pagination
{
    public static class PaginationHelper
    {
        public static PagedQuery<T> Paginate<T>(this IQueryable<T> query, QueryFilter filter)
        {
            return new PagedQuery<T>(query, filter);
        }

        internal static IPagedQuery Paginate(this IQueryable query, QueryFilter filter)
        {
            var pagedType = typeof(PagedQuery<>).MakeGenericType(query.ElementType);
            var pagedQuery = Activator.CreateInstance(pagedType, query, filter);

            return (IPagedQuery)pagedQuery;
        }
    }
}