using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IronbugCore.Context;

public static class ContextHelper
{
    /// <summary>
    /// Insere a entidade quando o <see cref="IEntity.Id"/> ainda é <see cref="Guid.Empty"/>;
    /// caso contrário, atualiza-a.
    /// </summary>
    public static void AddOrUpdate<T>(this DbSet<T> dbSet, T entity) where T : class, IEntity
    {
        var context = dbSet.GetDbContext();

        if (entity.Id == Guid.Empty)
            context.Set<T>().Add(entity);
        else
        {
            context.Entry(entity).State = EntityState.Detached;
            context.Set<T>().Update(entity);
        }
    }

    /// <summary>
    /// Cria um contexto efêmero a partir da factory, executa <paramref name="work"/>
    /// e o descarta — garantindo o ciclo de vida correto fora do escopo do request.
    /// </summary>
    public static async Task<TResult> RunWithLocalContextAsync<TContext, TResult>(
        this IContextFactory<TContext> factory,
        Func<TContext, Task<TResult>> work)
        where TContext : IContext
    {
        await using var context = factory.Create();
        return await work(context);
    }

    private static DbContext GetDbContext<T>(this DbSet<T> dbSet) where T : class
    {
        var serviceProvider = ((IInfrastructure<IServiceProvider>)dbSet).Instance;
        var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
        return currentDbContext?.Context
               ?? throw new InvalidOperationException("Não foi possível resolver o DbContext atual a partir do DbSet.");
    }
}
