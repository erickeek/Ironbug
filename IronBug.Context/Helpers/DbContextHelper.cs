using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronBug.Context.Helpers
{
    public static class DbContextHelper
    {
        public static TDbContext BulkInsert<TDbContext, TEntity>(this TDbContext context, IEnumerable<TEntity> entities, DbContextOptions options = null)
            where TDbContext : DbContext
            where TEntity : class
        {
            var count = 0;

            options = options ?? new DbContextOptions();
            context.Configuration.AutoDetectChangesEnabled = options.AutoDetectChangesEnabled;
            context.Configuration.ValidateOnSaveEnabled = options.ValidateOnSaveEnabled;

            foreach (var entity in entities)
            {
                count++;
                context.Set<TEntity>().Add(entity);

                if (count % options.CommitCount == 0)
                {
                    context.SaveChanges();
                    context.Dispose();
                    context = Activator.CreateInstance<TDbContext>();
                    context.Configuration.AutoDetectChangesEnabled = options.AutoDetectChangesEnabled;
                    context.Configuration.ValidateOnSaveEnabled = options.ValidateOnSaveEnabled;
                }
            }

            context.SaveChanges();
            return context;
        }

        public static void Truncate<TEntity>(this DbContext context) where TEntity : class
        {
            context.Database.ExecuteSqlCommand($"TRUNCATE TABLE {typeof(TEntity).Name}");
        }

        public static void DeleteAll<TEntity>(this DbContext context) where TEntity : class
        {
            var tableName = typeof(TEntity).Name;
            context.Database.ExecuteSqlCommand(
                $@"DELETE FROM {tableName}
                DBCC CHECKIDENT ('{tableName}', reseed, 0)"
            );
        }

        public static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class
        {
            var internalSet = dbSet.GetType().GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dbSet);
            var internalContext = internalSet?.GetType().BaseType?.GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(internalSet);
            return (DbContext)internalContext?.GetType().GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public).GetValue(internalContext, null);
        }

        public static void SaveManyToMany<TRoot, TChild>(this DbSet<TRoot> dbSet,
            IEnumerable<TChild> currentData,
            Expression<Func<TRoot, int>> primaryKeyRoot,
            Expression<Func<TChild, int>> primaryKeyChild,
            Expression<Func<TRoot, ICollection<TChild>>> databaseData,
            int primaryKeyRootId)
            where TRoot : class
            where TChild : class
        {
            var propertyName = ((MemberExpression)primaryKeyRoot.Body).Member.Name;
            var compiledPrimaryKeyChild = primaryKeyChild.Compile();

            var context = dbSet.GetContext();
            var entityInDatabase = EntityInDatabase(dbSet, primaryKeyRootId, propertyName);
            var collection = databaseData.Compile()(entityInDatabase);
            collection.Clear();

            var ids = currentData.Select(compiledPrimaryKeyChild).ToList();

            var databaseEntities = DatabaseEntities(context, primaryKeyChild, ids);

            foreach (TChild entity in databaseEntities)
            {
                collection.Add(entity);
            }

            context.SaveChanges();
        }

        private static IQueryable DatabaseEntities<TChild>(DbContext context, Expression<Func<TChild, int>> primaryKeyChild, IReadOnlyCollection<int> ids) where TChild : class
        {
            var methodInfo = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
            var parameter = Expression.Parameter(typeof(TChild), "x");
            var member = Expression.Property(parameter, ((MemberExpression)primaryKeyChild.Body).Member.Name);
            var constant = Expression.Constant(ids);
            var body = Expression.Call(constant, methodInfo, member);
            var wherePredicate = Expression.Lambda<Func<TChild, bool>>(body, parameter);
            var databaseEntities = context.Set<TChild>().Where(wherePredicate);
            return databaseEntities;
        }

        private static TRoot EntityInDatabase<TRoot>(DbSet<TRoot> dbSet, int primaryKeyRootId, string propertyName) where TRoot : class
        {
            var parameter = Expression.Parameter(typeof(TRoot), "x");
            var property = Expression.Property(parameter, propertyName);
            var target = Expression.Constant(primaryKeyRootId);
            var equalsMethod = Expression.Call(property, "Equals", null, target);
            var wherePredicate = Expression.Lambda<Func<TRoot, bool>>(equalsMethod, parameter);

            var entityInDatabase = dbSet.First(wherePredicate);
            return entityInDatabase;
        }

        public static void SaveOneToMany<T>(this DbSet<T> dbSet,
            List<T> currentData,
            Expression<Func<T, int>> primaryKey,
            Expression<Func<T, int>> foreignKey,
            int foreignKeyId) where T : class
        {
            var entityType = typeof(T);
            var propertyName = ((MemberExpression)foreignKey.Body).Member.Name;
            var compiledPrimaryKey = primaryKey.Compile();

            var context = dbSet.GetContext();

            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, propertyName);
            var target = Expression.Constant(foreignKeyId);
            var equalsMethod = Expression.Call(property, "Equals", null, target);
            var wherePredicate = Expression.Lambda<Func<T, bool>>(equalsMethod, parameter);

            var entitiesInDatabase = dbSet.Where(wherePredicate);
            var databaseIds = entitiesInDatabase.Select(primaryKey).ToList();
            var currentIds = currentData.Select(compiledPrimaryKey).Where(q => q > 0).ToList();
            var idsToRemove = databaseIds.Except(currentIds).ToList();

            dbSet.RemoveRange(entitiesInDatabase.ToList().Where(q => idsToRemove.Contains(compiledPrimaryKey.Invoke(q))));

            foreach (var entity in currentData)
            {
                var propertyInfo = entityType.GetProperty(propertyName);
                if (propertyInfo == null) continue;

                propertyInfo.SetValue(entity, foreignKeyId);

                dbSet.AddOrUpdate(entity);
            }

            context.SaveChanges();
        }
    }
}