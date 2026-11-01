using Microsoft.AspNetCore.Identity;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Web.Autorizacao;

public sealed class UsuarioAtual(IHttpContextAccessor contexto, UserManager<IdentityUser> usuarios) : IUsuarioAtual
{
    public async ValueTask<ContaAutenticada?> ObterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var identidade = contexto.HttpContext?.User;
        if (identidade?.Identity?.IsAuthenticated != true)
            return null;
        var usuario = await usuarios.GetUserAsync(identidade);
        if (usuario is null)
            return null;
        // O cookie identifica a conta; o perfil vigente é lido do store em cada operação.
        var claims = await usuarios.GetClaimsAsync(usuario);
        cancellationToken.ThrowIfCancellationRequested();
        var perfis = claims.Where(c => c.Type == PerfisNegocio.TipoClaim).ToArray();
        return new(usuario.Id, perfis.Length == 1 ? perfis[0].Value : null);
    }
}
