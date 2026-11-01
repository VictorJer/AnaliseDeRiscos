using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class ConsultarEdicaoRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<DadosEdicaoRebanhoDto>> ExecutarAsync(ConsultarRebanhoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var consulta = await acesso.ConsultarAsync(entrada.Id, cancellationToken);
        if (consulta.IsFailed)
            return Result.Fail<DadosEdicaoRebanhoDto>(consulta.Errors);
        var rebanho = consulta.Value.Rebanho;
        return Result.Ok(new DadosEdicaoRebanhoDto(rebanho.Id, rebanho.Versao, rebanho.Nome, rebanho.Propriedade, rebanho.Analise.Entradas));
    }
}
