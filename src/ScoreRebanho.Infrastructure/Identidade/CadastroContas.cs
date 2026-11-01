using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Infrastructure.Identidade;

public sealed class CadastroContas(UserManager<IdentityUser> usuarios, ContasDbContext contexto) : ICadastroContas
{
    public async Task<CadastroPersistido> CadastrarAsync(CadastrarContaDto dados, CancellationToken cancellationToken)
    {
        await using var transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        var usuario = new IdentityUser { UserName = dados.Email, Email = dados.Email };
        IdentityResult criacao;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            criacao = await usuarios.CreateAsync(usuario, dados.Senha);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 })
        {
            return new(null, [new("Email", "Não foi possível cadastrar este e-mail. Tente entrar com sua conta.")]);
        }
        if (!criacao.Succeeded)
            return new(null, criacao.Errors.Select(e => new FalhaCadastro(CampoErro(e.Code), e.Description)).ToArray());

        // Conta e perfil são gravados juntos; o cliente não escolhe permissões.
        var perfil = await usuarios.AddClaimAsync(usuario, new Claim(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista));
        if (!perfil.Succeeded)
            throw new InvalidOperationException("Não foi possível atribuir o perfil inicial da conta.");
        await transacao.CommitAsync(cancellationToken);
        return new(usuario.Id, []);
    }

    private static string CampoErro(string codigo) =>
        codigo.StartsWith("Password", StringComparison.Ordinal) ? "Senha" : "Email";
}
