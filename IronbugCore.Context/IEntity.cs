namespace IronbugCore.Context;

/// <summary>
/// Marca uma entidade de domínio com chave primária <see cref="Guid"/>.
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}
