using FluentResults;

namespace ScoreRebanho.Application.Contas;

internal static class AutorizacaoConta
{
    internal static async Task<Result<ContaAutenticada>> ObterAsync(IUsuarioAtual usuarioAtual, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var conta = await usuarioAtual.ObterAsync(cancellationToken);
        if (conta is null || string.IsNullOrWhiteSpace(conta.Id))
            return Result.Fail<ContaAutenticada>(new Error("Entre na sua conta para acessar os rebanhos.").WithMetadata("Codigo", "NAO_AUTENTICADO"));
        if (conta.PerfilNegocio is not (PerfisNegocio.Pecuarista or PerfisNegocio.Cooperativa))
            return Result.Fail<ContaAutenticada>(new Error("Sua conta não possui um perfil de negócio válido.").WithMetadata("Codigo", "PERFIL_INVALIDO"));
        return Result.Ok(conta);
    }
}
