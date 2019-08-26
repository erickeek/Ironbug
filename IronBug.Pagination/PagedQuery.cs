using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronBug.Pagination
{
    public interface IPagedQuery : IQueryable
    {
        int Total { get; }
    }

    public class PagedQuery<T> : IQueryable<T>, IPagedQuery
    {
        private readonly IQueryable<T> _query;
        private readonly QueryFilter _filter;
        private readonly Sorter<T> _sorter;
        private IQueryable<T> _new;

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
        public int Total => _query.Count();

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
    }
}