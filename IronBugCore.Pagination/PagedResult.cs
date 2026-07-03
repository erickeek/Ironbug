namespace IronbugCore.Pagination;

/// <summary>
/// Resultado de paginação já materializado: a página de itens mais a contagem total
/// (ambas obtidas de forma assíncrona). Use-o para evitar a propriedade síncrona
/// <see cref="PagedQuery{T}.Total"/> em fluxos assíncronos.
/// </summary>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int CurrentPage { get; init; }
    public int ItemsPerPage { get; init; }

    public int TotalPages => ItemsPerPage <= 0 ? 0 : (int)Math.Ceiling(Total / (double)ItemsPerPage);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
