using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Administracao;

public sealed class ConsultarScoreAdministrativo(IUsuarioAtual usuarioAtual, IConsultaScoresAdministrativos consulta)
{
    public async Task<Result<RelatorioScoreAdministrativoDto>> ExecutarAsync(ConsultarScoreAdministrativoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var acesso = await AcessoAdministrativo.ValidarAsync(usuarioAtual, cancellationToken);
        if (acesso.IsFailed) return Result.Fail<RelatorioScoreAdministrativoDto>(acesso.Errors);
        var relatorio = entrada.Id == Guid.Empty ? null : await consulta.ConsultarAsync(entrada.Id, cancellationToken);
        if (relatorio is null)
            return Result.Fail<RelatorioScoreAdministrativoDto>(new Error("Análise histórica não encontrada.").WithMetadata("Codigo", "NAO_ENCONTRADO"));
        return Result.Ok(relatorio);
    }
}
