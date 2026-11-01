using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Administracao;

public sealed class ListarScoresAdministrativos(IUsuarioAtual usuarioAtual, IConsultaScoresAdministrativos consulta)
{
    public async Task<Result<PaginaScoresAdministrativosDto>> ExecutarAsync(ListarScoresAdministrativosDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var acesso = await AcessoAdministrativo.ValidarAsync(usuarioAtual, cancellationToken);
        if (acesso.IsFailed) return Result.Fail<PaginaScoresAdministrativosDto>(acesso.Errors);
        if (entrada.Pagina <= 0)
            return Result.Fail<PaginaScoresAdministrativosDto>(new Error("Informe uma página maior que zero.")
                .WithMetadata("Codigo", "ENTRADA_INVALIDA").WithMetadata("Campo", "Pagina"));
        return Result.Ok(await consulta.ListarAsync(entrada.Pagina, cancellationToken));
    }
}
