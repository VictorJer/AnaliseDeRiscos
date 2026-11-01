using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class AtualizarRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio, CalcularScoreRebanho calcular)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<RebanhoGravadoDto>> ExecutarAsync(AtualizarRebanhoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var acessoAtual = await acesso.ConsultarAsync(entrada.Id, cancellationToken);
        if (acessoAtual.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(acessoAtual.Errors);
        if (entrada.Versao != acessoAtual.Value.Rebanho.Versao)
            return Result.Fail<RebanhoGravadoDto>(FalhasRebanhos.Conflito());
        var identificacao = FalhasRebanhos.Identificacao(entrada.Nome, entrada.Propriedade);
        if (identificacao.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(identificacao.Errors);
        if (entrada.Entradas is null)
            return Result.Fail<RebanhoGravadoDto>(FalhasRebanhos.Entrada("Entradas", "Informe os dados da simulação."));
        var analise = calcular.Executar(entrada.Entradas);
        if (analise.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(analise.Errors);
        var gravacao = await repositorio.AtualizarAsync(entrada.Id, acessoAtual.Value.Usuario.Id,
            entrada.Versao, identificacao.Value, analise.Value, cancellationToken);
        return FalhasRebanhos.Gravacao(gravacao);
    }
}
