using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Simulacoes;

public sealed record ConferenciaOcupacaoDto(string VersaoPremissas, LotacaoAnaliseDto Lotacao);

public sealed class ConferirOcupacao(IUsuarioAtual usuarioAtual, CalcularScoreRebanho calculadora)
{
    public async Task<Result<ConferenciaOcupacaoDto>> ExecutarAsync(EntradaSimulacaoDto entrada, CancellationToken cancellationToken = default)
    {
        var usuario = await AutorizacaoConta.ObterAsync(usuarioAtual, cancellationToken);
        if (usuario.IsFailed)
            return Result.Fail<ConferenciaOcupacaoDto>(usuario.Errors);
        return calculadora.Conferir(entrada);
    }
}
