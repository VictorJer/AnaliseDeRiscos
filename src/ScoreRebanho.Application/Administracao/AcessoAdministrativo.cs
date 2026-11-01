using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Administracao;

internal static class AcessoAdministrativo
{
    internal static async Task<Result> ValidarAsync(IUsuarioAtual usuarioAtual, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null || string.IsNullOrWhiteSpace(usuario.Id))
            return Result.Fail(new Error("Entre na sua conta para consultar os scores.").WithMetadata("Codigo", "NAO_AUTENTICADO"));
        if (!usuario.Administrador)
            return Result.Fail(new Error("A consulta administrativa exige o papel Administrador.").WithMetadata("Codigo", "NAO_AUTORIZADO"));
        return Result.Ok();
    }
}
