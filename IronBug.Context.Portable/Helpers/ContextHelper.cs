using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IronBug.Context.Helpers
{
    public static class ContextHelper
    {
        public static void AddOrUpdate<T>(this DbSet<T> dbset, IEntity entity) where T : class
        {
            var context = dbset.GetDbContext();

            if (entity.Id == 0)
                context.Add(entity);
            else
                context.Update(entity);
        }

        private static Microsoft.EntityFrameworkCore.DbContext GetDbContext<T>(this DbSet<T> dbSet) where T : class
        {
            var infrastructure = dbSet as IInfrastructure<IServiceProvider>;
            var serviceProvider = infrastructure.Instance;
            var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
            return currentDbContext?.Context;
        }
    }
}