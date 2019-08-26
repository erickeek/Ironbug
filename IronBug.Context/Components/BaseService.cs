using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;

namespace IronBug.Context.Components
{
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected BaseService(DbContext context)
        {
            Context = context;
            Set = Context.Set<TEntity>();
        }

        public DbContext Context { get; }
        protected DbSet<TEntity> Set { get; set; }

        public virtual void Save(TEntity entity)
        {
            Set.AddOrUpdate(entity);
            Context.SaveChanges();
        }
        public virtual void Add(TEntity entity)
        {
            Set.Add(entity);
            Context.SaveChanges();
        }
        public virtual void Update(TEntity entity)
        {
            var entry = Context.Entry(entity);
            Set.Attach(entity);
            entry.State = EntityState.Modified;
            Context.SaveChanges();
        }
        public virtual void Delete(int id)
        {
            Set.Remove(Find(id));
            Context.SaveChanges();
        }
        public virtual void Delete(TEntity entity)
        {
            Set.Remove(entity);
            Context.SaveChanges();
        }
        public virtual TEntity Find(int id)
        {
            return id <= 0 ? null : Set.Find(id);
        }
        public virtual IQueryable<TEntity> All(bool @readonly = false)
        {
            return @readonly ? Set.AsNoTracking() : Set;
        }
        public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, bool @readonly = false)
        {
            return @readonly ? Set.Where(predicate).AsNoTracking() : Set.Where(predicate);
        }
    }
}