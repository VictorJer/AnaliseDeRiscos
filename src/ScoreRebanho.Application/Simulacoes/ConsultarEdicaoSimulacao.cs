using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;

namespace ScoreRebanho.Application.Simulacoes;

public sealed record ConsultarEdicaoSimulacaoDto(Guid Id);
public sealed record DadosEdicaoSimulacaoDto(EntradaSimulacaoDto Entradas);

public sealed class ConsultarEdicaoSimulacao(IUsuarioAtual usuarioAtual, IRepositorioScores repositorio)
{
    public async Task<Result<DadosEdicaoSimulacaoDto>> ExecutarAsync(ConsultarEdicaoSimulacaoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var usuario = await AutorizacaoConta.ObterAsync(usuarioAtual, cancellationToken);
        if (usuario.IsFailed) return Result.Fail<DadosEdicaoSimulacaoDto>(usuario.Errors);
        var historico = entrada.Id == Guid.Empty ? null
            : await repositorio.ConsultarDoAutorAsync(entrada.Id, usuario.Value.Id, cancellationToken);
        if (historico is null || historico.RebanhoOrigemId is not null)
            return Result.Fail<DadosEdicaoSimulacaoDto>(new Error("Simulação avulsa não encontrada.").WithMetadata("Codigo", "NAO_ENCONTRADO"));
        return Result.Ok(new DadosEdicaoSimulacaoDto(historico.Analise.Entradas));
    }
}
