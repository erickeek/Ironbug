using System.Diagnostics.CodeAnalysis;

namespace IronbugCore.Common;

/// <summary>
/// Resultado de uma operação que pode falhar, sem lançar exceção para fluxo esperado.
/// </summary>
public record Result
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    protected Result()
    {
    }

    public static Result Ok() => new() { Success = true };

    [MemberNotNull(nameof(Error))]
    public static Result Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// <see cref="Result"/> que carrega um dado em caso de sucesso.
/// </summary>
public sealed record Result<T> : Result
{
    public T? Data { get; set; }

    private Result()
    {
    }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };

    public new static Result<T> Fail(string error) => new() { Success = false, Error = error };
}
