namespace ScoreRebanho.Application.Contas;

public interface IUsuarioAtual
{
    ValueTask<ContaAutenticada?> ObterAsync(CancellationToken cancellationToken = default);
}

public sealed record ContaAutenticada(string Id, string? PerfilNegocio, bool Administrador = false);
