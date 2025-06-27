namespace IronBugCore.Pagination;

public record QueryFilter
{
    public string? Search { get; set; }
    public string? Predicate { get; set; }
    public bool Reverse { get; set; } = false;

    public int ItemsPerPage { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
}