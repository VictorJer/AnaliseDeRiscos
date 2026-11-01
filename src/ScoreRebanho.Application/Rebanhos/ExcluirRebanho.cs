using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class ExcluirRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result> ExecutarAsync(ExcluirRebanhoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var acessoAtual = await acesso.ConsultarAsync(entrada.Id, cancellationToken);
        if (acessoAtual.IsFailed)
            return Result.Fail(acessoAtual.Errors);
        if (entrada.Versao != acessoAtual.Value.Rebanho.Versao)
            return Result.Fail(FalhasRebanhos.Conflito());
        var estado = await repositorio.ExcluirAsync(entrada.Id, acessoAtual.Value.Usuario.Id, entrada.Versao, cancellationToken);
        return FalhasRebanhos.Exclusao(estado);
    }
}
