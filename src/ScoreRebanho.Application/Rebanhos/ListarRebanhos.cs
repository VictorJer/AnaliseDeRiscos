using FluentResults;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Rebanhos;

public sealed class ListarRebanhos(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<PaginaRebanhosDto>> ExecutarAsync(ListarRebanhosDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var usuario = await acesso.UsuarioAsync(cancellationToken);
        if (usuario.IsFailed)
            return Result.Fail<PaginaRebanhosDto>(usuario.Errors);
        if (entrada.Pagina <= 0)
            return Result.Fail<PaginaRebanhosDto>(FalhasRebanhos.Entrada("Pagina", "Informe uma página maior que zero."));
        return Result.Ok(await repositorio.ListarAsync(usuario.Value.Id, entrada.Pagina, cancellationToken));
    }
}
