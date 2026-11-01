using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Relatorios;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class ConsultarRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<RelatorioRebanhoDto>> ExecutarAsync(ConsultarRebanhoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var consulta = await acesso.ConsultarAsync(entrada.Id, cancellationToken);
        if (consulta.IsFailed)
            return Result.Fail<RelatorioRebanhoDto>(consulta.Errors);
        var rebanho = consulta.Value.Rebanho;
        return Result.Ok(new RelatorioRebanhoDto(rebanho.Id, rebanho.Nome, rebanho.Propriedade, rebanho.UltimoScoreId,
            ProjecaoRelatorio.Criar(rebanho.Analise, consulta.Value.Usuario)));
    }
}
