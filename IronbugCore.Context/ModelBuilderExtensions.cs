using Microsoft.EntityFrameworkCore;

namespace IronbugCore.Context;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Usa o nome da classe (singular) como nome de tabela, em vez do nome do
    /// <see cref="DbSet{TEntity}"/> (que costuma ser plural). Chame no <c>OnModelCreating</c>.
    /// </summary>
    public static ModelBuilder UseSingularTableNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.ClrType.Name.Contains("Dictionary"))
                continue;

            entity.SetTableName(entity.DisplayName());
        }

        return modelBuilder;
    }
}
