using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace IronbugCore.Pagination;

public interface IPagedQuery : IQueryable
{
    int Total { get; }
}

public sealed class PagedQuery<T> : IQueryable<T>, IPagedQuery, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _query;
    private readonly QueryFilter _filter;
    private readonly Sorter<T> _sorter;
    private IQueryable<T>? _new;
    private int? _total;

    public PagedQuery(IQueryable<T> query, QueryFilter filter)
    {
        _query = query;
        _filter = filter;
        _sorter = new Sorter<T>();
    }

    private IQueryable<T> NewQuery
    {
        get
        {
            if (_new == null)
            {
                _new = _sorter.Apply(_query, _filter.Predicate, _filter.Reverse);
                var skip = _filter.ItemsPerPage * (_filter.CurrentPage - 1);
                _new = _new.Skip(skip).Take(_filter.ItemsPerPage);
            }

            return _new;
        }
    }

    /// <remarks>
    /// Executa um <c>COUNT</c> síncrono na primeira leitura (bloqueante). Em código
    /// assíncrono, prefira <see cref="GetTotalAsync"/> ou, melhor ainda,
    /// <see cref="ToPagedResultAsync(CancellationToken)"/>, que traz itens + total sem bloquear thread.
    /// </remarks>
    public int Total => _total ??= _query.Count();

    public async Task<int> GetTotalAsync(CancellationToken cancellationToken = default)
    {
        if (_total.HasValue)
            return _total.Value;

        _total = await _query.CountAsync(cancellationToken);
        return _total.Value;
    }

    /// <summary>
    /// Materializa de forma totalmente assíncrona a página atual e a contagem total,
    /// sem acionar a propriedade síncrona <see cref="Total"/>.
    /// </summary>
    public async Task<PagedResult<T>> ToPagedResultAsync(CancellationToken cancellationToken = default)
    {
        var total = await GetTotalAsync(cancellationToken);
        var items = await NewQuery.ToListAsync(cancellationToken);
        return BuildResult(items, total);
    }

    /// <summary>
    /// Igual a <see cref="ToPagedResultAsync(CancellationToken)"/>, mas projetando a página
    /// para <typeparamref name="TResult"/> direto no banco (a projeção entra no SQL).
    /// </summary>
    public async Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(
        Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
    {
        var total = await GetTotalAsync(cancellationToken);
        var items = await NewQuery.Select(selector).ToListAsync(cancellationToken);
        return new PagedResult<TResult>
        {
            Items = items,
            Total = total,
            CurrentPage = _filter.CurrentPage,
            ItemsPerPage = _filter.ItemsPerPage
        };
    }

    private PagedResult<TItem> BuildResult<TItem>(IReadOnlyList<TItem> items, int total) => new()
    {
        Items = items,
        Total = total,
        CurrentPage = _filter.CurrentPage,
        ItemsPerPage = _filter.ItemsPerPage
    };

    public void AddSorterField<TKey>(Expression<Func<T, TKey>> expression)
    {
        _sorter.AddPredicate(expression);
    }

    public void AddSorterField<TKey>(string predicate, Expression<Func<T, TKey>> expression)
    {
        _sorter.AddPredicate(predicate, expression);
    }

    #region IQueryable

    public Expression Expression => NewQuery.Expression;
    public Type ElementType => NewQuery.ElementType;
    public IQueryProvider Provider => NewQuery.Provider;

    public IEnumerator<T> GetEnumerator()
    {
        return NewQuery.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region IAsyncEnumerable

    // Delega para o provider subjacente (ex.: EF Core), preservando a tradução
    // assíncrona da query paginada. Assim, ToListAsync/await foreach funcionam
    // diretamente sobre o PagedQuery, e a contagem total continua vindo de _query.
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (NewQuery is IAsyncEnumerable<T> asyncQuery)
            return asyncQuery.GetAsyncEnumerator(cancellationToken);

        throw new NotSupportedException(
            "A fonte de dados subjacente não suporta enumeração assíncrona (use um provider como o EF Core).");
    }

    #endregion
}