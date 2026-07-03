using System.Linq.Expressions;
using System.Reflection;

namespace IronbugCore.Pagination;

/// <summary>
/// Executa <c>Count</c>/<c>ToList</c> de forma assíncrona quando o provider do
/// <see cref="IQueryable{T}"/> suporta (ex.: EF Core, via <c>IAsyncQueryProvider</c> e
/// <see cref="IAsyncEnumerable{T}"/>), <b>sem depender do pacote EF Core em tempo de compilação</b>.
/// Quando o provider não é assíncrono, cai para execução síncrona.
/// </summary>
internal static class AsyncQueryableExecutor
{
    public static async Task<List<T>> ToListAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        if (source is not IAsyncEnumerable<T> asyncEnumerable)
            return source.ToList();

        var list = new List<T>();
        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                list.Add(enumerator.Current);
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        return list;
    }

    public static async Task<int> CountAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        var provider = source.Provider;
        var asyncProviderInterface = provider.GetType().GetInterfaces()
            .FirstOrDefault(i => i.FullName == "Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider");

        var executeAsync = asyncProviderInterface?.GetMethod("ExecuteAsync");
        if (executeAsync == null)
            return source.Count();

        var countExpression = Expression.Call(
            typeof(Queryable), nameof(Queryable.Count), [typeof(T)], source.Expression);

        try
        {
            var task = (Task<int>)executeAsync
                .MakeGenericMethod(typeof(Task<int>))
                .Invoke(provider, [countExpression, cancellationToken])!;

            return await task.ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }
}
