using System;
using System.Collections.Generic;
using System.Data.Entity;

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
    }
}