using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;

namespace ScoreRebanho.Application.Simulacoes;

public sealed record GerarSimulacaoDto(EntradaSimulacaoDto Entradas);
public sealed record SimulacaoGravadaDto(Guid Id);

public sealed class GerarSimulacao(IUsuarioAtual usuarioAtual, IRepositorioScores repositorio, CalcularScoreRebanho calcular)
{
    public async Task<Result<SimulacaoGravadaDto>> ExecutarAsync(GerarSimulacaoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var usuario = await AutorizacaoConta.ObterAsync(usuarioAtual, cancellationToken);
        if (usuario.IsFailed) return Result.Fail<SimulacaoGravadaDto>(usuario.Errors);
        if (entrada.Entradas is null)
            return Result.Fail<SimulacaoGravadaDto>(new Error("Informe os dados da simulação.")
                .WithMetadata("Codigo", "ENTRADA_INVALIDA").WithMetadata("Campo", "Entradas"));
        var analise = calcular.Executar(entrada.Entradas);
        if (analise.IsFailed) return Result.Fail<SimulacaoGravadaDto>(analise.Errors);
        var id = await repositorio.SalvarAvulsoAsync(usuario.Value.Id, analise.Value, cancellationToken);
        return Result.Ok(new SimulacaoGravadaDto(id));
    }
}
