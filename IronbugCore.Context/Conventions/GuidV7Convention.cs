using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace IronbugCore.Context.Conventions;

/// <summary>
/// Aplica o <see cref="GuidV7ValueGenerator"/> a toda chave primária do tipo <see cref="Guid"/>.
/// Registre via <see cref="ModelConfigurationBuilderExtensions.UseGuidV7Keys"/>.
/// </summary>
public sealed class GuidV7Convention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
                continue;

            foreach (var property in primaryKey.Properties)
            {
                if (property.ClrType == typeof(Guid))
                    property.Builder.HasValueGenerator((_, _) => new GuidV7ValueGenerator());
            }
        }
    }
}
