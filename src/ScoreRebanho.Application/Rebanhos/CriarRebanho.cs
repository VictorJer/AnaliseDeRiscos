using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class CriarRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio, CalcularScoreRebanho calcular)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<RebanhoGravadoDto>> ExecutarAsync(CriarRebanhoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var usuario = await acesso.UsuarioAsync(cancellationToken);
        if (usuario.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(usuario.Errors);
        var identificacao = FalhasRebanhos.Identificacao(entrada.Nome, entrada.Propriedade);
        if (identificacao.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(identificacao.Errors);
        if (entrada.Entradas is null)
            return Result.Fail<RebanhoGravadoDto>(FalhasRebanhos.Entrada("Entradas", "Informe os dados da simulação."));
        var localizacao = FalhasRebanhos.Localizacao(entrada.Latitude, entrada.Longitude);
        if (localizacao.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(localizacao.Errors);
        var analise = calcular.Executar(entrada.Entradas);
        if (analise.IsFailed)
            return Result.Fail<RebanhoGravadoDto>(analise.Errors);
        var salvo = await repositorio.CriarAsync(usuario.Value.Id, identificacao.Value, analise.Value, cancellationToken, localizacao.Value);
        return Result.Ok(salvo);
    }
}
