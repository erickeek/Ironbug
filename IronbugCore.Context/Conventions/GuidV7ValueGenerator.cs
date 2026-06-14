using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace IronbugCore.Context.Conventions;

/// <summary>
/// Gera chaves <see cref="Guid"/> versão 7 (ordenáveis por tempo) no cliente,
/// melhorando a localidade de índice em bancos relacionais.
/// </summary>
public sealed class GuidV7ValueGenerator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry)
    {
        return Guid.CreateVersion7();
    }
}
