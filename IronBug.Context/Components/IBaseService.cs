using System;
using System.Linq;
using System.Linq.Expressions;

namespace IronBug.Context.Components
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        void Save(TEntity entity);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(int id);
        void Delete(TEntity entity);
        TEntity Find(int id);
        IQueryable<TEntity> All(bool @readonly = false);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, bool @readonly = false);
    }
}