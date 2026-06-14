using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IronbugCore.Context;

/// <summary>
/// Abstração mínima de um DbContext, para desacoplar o domínio do EF Core concreto
/// e permitir testes/factory. O DbContext da aplicação implementa esta interface
/// (normalmente estendendo-a com os <see cref="DbSet{TEntity}"/> nomeados do projeto).
/// </summary>
public interface IContext : IDisposable, IAsyncDisposable
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
