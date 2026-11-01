using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Rebanhos;

internal sealed class AcessoRebanhos(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio)
{
    internal Task<Result<ContaAutenticada>> UsuarioAsync(CancellationToken cancellationToken) =>
        AutorizacaoConta.ObterAsync(usuarioAtual, cancellationToken);
    internal async Task<Result<RebanhoDoUsuario>> ConsultarAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await UsuarioAsync(cancellationToken);
        if (usuario.IsFailed)
            return Result.Fail<RebanhoDoUsuario>(usuario.Errors);
        if (id == Guid.Empty)
            return Result.Fail<RebanhoDoUsuario>(FalhasRebanhos.NaoEncontrado());
        var rebanho = await repositorio.ConsultarAsync(id, usuario.Value.Id, cancellationToken);
        return rebanho is null
            ? Result.Fail<RebanhoDoUsuario>(FalhasRebanhos.NaoEncontrado())
            : Result.Ok(new RebanhoDoUsuario(usuario.Value, rebanho));
    }
}

internal sealed record RebanhoDoUsuario(ContaAutenticada Usuario, RebanhoPersistidoDto Rebanho);
