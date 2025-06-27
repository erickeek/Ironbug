using System.Linq.Expressions;

namespace IronBugCore.Pagination;

public sealed class Sorter<T>
{
    private delegate IQueryable<T> Filter(IQueryable<T> query, bool reverse);

    private readonly IDictionary<string, Filter> _expressions;

    public Sorter()
    {
        _expressions = new Dictionary<string, Filter>();
    }

    public void AddPredicate<TKey>(string predicate, Expression<Func<T, TKey>> expression)
    {
        _expressions.Add(predicate, (query, reverse) => reverse ? query.OrderByDescending(expression) : query.OrderBy(expression));
    }

    public void AddPredicate<TKey>(Expression<Func<T, TKey>> expression)
    {
        var predicate = "";
        var member = (MemberExpression)expression.Body;
        while (member != null)
        {
            if (predicate != "")
                predicate = $".{predicate}";

            predicate = $"{member.Member.Name}{predicate}";

            member = member.Expression as MemberExpression;
        }

        AddPredicate(predicate, expression);
    }

    public IQueryable<T> Apply(IQueryable<T> query, string? predicate, bool reverse)
    {
        if (predicate == null)
            return query;

        if (!_expressions.ContainsKey(predicate))
            predicate = _expressions.Keys.First();

        return _expressions[predicate](query, reverse);
    }
}