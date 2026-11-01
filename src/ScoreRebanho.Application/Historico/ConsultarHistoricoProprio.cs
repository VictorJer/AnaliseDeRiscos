using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Relatorios;


namespace ScoreRebanho.Application.Historico;

public sealed record ConsultarHistoricoDto(Guid Id);

public sealed class ConsultarHistoricoProprio(IUsuarioAtual usuarioAtual, IRepositorioScores repositorio)
{
    public async Task<Result<RelatorioHistoricoDto>> ExecutarAsync(ConsultarHistoricoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var usuario = await AutorizacaoConta.ObterAsync(usuarioAtual, cancellationToken);
        if (usuario.IsFailed)
            return Result.Fail<RelatorioHistoricoDto>(usuario.Errors);
        var historico = entrada.Id == Guid.Empty ? null
            : await repositorio.ConsultarDoAutorAsync(entrada.Id, usuario.Value.Id, cancellationToken);
        if (historico is null)
            return Result.Fail<RelatorioHistoricoDto>(new Error("Análise histórica não encontrada.").WithMetadata("Codigo", "NAO_ENCONTRADO"));
        return Result.Ok(new RelatorioHistoricoDto(historico.Id, historico.RebanhoOrigemId,
            historico.NomeRebanho, historico.Propriedade, ProjecaoRelatorio.Criar(historico.Analise, usuario.Value)));
    }
}
