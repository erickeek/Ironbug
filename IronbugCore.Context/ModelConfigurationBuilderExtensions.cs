using IronbugCore.Context.Conventions;
using Microsoft.EntityFrameworkCore;

namespace IronbugCore.Context;

public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Registra a <see cref="GuidV7Convention"/>, gerando Guid v7 automaticamente
    /// para chaves primárias <see cref="Guid"/>. Chame no <c>ConfigureConventions</c> do seu DbContext.
    /// </summary>
    public static ModelConfigurationBuilder UseGuidV7Keys(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new GuidV7Convention());
        return configurationBuilder;
    }
}
