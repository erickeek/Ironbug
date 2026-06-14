namespace IronbugCore.Context;

/// <summary>
/// Cria instâncias de contexto sob demanda — útil para background jobs (Hangfire),
/// operações paralelas ou qualquer fluxo fora do escopo de DI do request.
/// </summary>
/// <typeparam name="TContext">O tipo de contexto da aplicação (deve implementar <see cref="IContext"/>).</typeparam>
public interface IContextFactory<out TContext> where TContext : IContext
{
    TContext Create();
}
