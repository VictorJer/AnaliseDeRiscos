using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Web.Comandos;

public sealed class ExecutarComandoContaLocal(UserManager<IdentityUser> usuarios,
    RoleManager<IdentityRole> papeis, ILogger<ExecutarComandoContaLocal> logger)
{
    private const string Administrador = "Administrador";

    public async Task<ResultadoComandoLocal> ExecutarAsync(ComandoContaLocal comando)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var usuario = await usuarios.FindByEmailAsync(comando.Email);
        if (usuario is null) return new(3, "Conta não encontrada. Cadastre a conta antes de atribuir acessos.");
        return comando switch
        {
            DefinirPerfilLocal perfil => await DefinirPerfilAsync(usuario, perfil.Perfil),
            ConcederAdministradorLocal => await ConcederAdministradorAsync(usuario),
            _ => throw new InvalidOperationException("Comando local não suportado.")
        };
    }

    private async Task<ResultadoComandoLocal> DefinirPerfilAsync(IdentityUser usuario, string perfil)
    {
        if (perfil is not (PerfisNegocio.Pecuarista or PerfisNegocio.Cooperativa))
            return new(2, "Informe Pecuarista ou Cooperativa como perfil.");
        var claims = (await usuarios.GetClaimsAsync(usuario)).Where(c => c.Type == PerfisNegocio.TipoClaim).ToArray();
        if (claims.Length != 1 || claims[0].Value is not (PerfisNegocio.Pecuarista or PerfisNegocio.Cooperativa))
            return new(1, "A conta não possui um único perfil válido. Revise o cadastro antes de atribuir acessos.");
        if (claims[0].Value == perfil) return new(0, "A conta já possui o perfil informado.");
        var resultado = await usuarios.ReplaceClaimAsync(usuario, claims[0], new Claim(PerfisNegocio.TipoClaim, perfil));
        if (!resultado.Succeeded) return Falha(resultado);
        logger.LogInformation("Perfil da conta {UsuarioId} alterado para {Perfil} por comando local.", usuario.Id, perfil);
        return new(0, "Perfil atualizado com sucesso.");
    }

    private async Task<ResultadoComandoLocal> ConcederAdministradorAsync(IdentityUser usuario)
    {
        if (await usuarios.IsInRoleAsync(usuario, Administrador))
            return new(0, "A conta já possui o papel Administrador.");
        if (!await papeis.RoleExistsAsync(Administrador))
        {
            var criacao = await papeis.CreateAsync(new IdentityRole(Administrador));
            if (!criacao.Succeeded) return Falha(criacao);
        }
        var resultado = await usuarios.AddToRoleAsync(usuario, Administrador);
        if (!resultado.Succeeded) return Falha(resultado);
        logger.LogInformation("Papel Administrador concedido à conta {UsuarioId} por comando local.", usuario.Id);
        return new(0, "Papel Administrador atribuído. Entre novamente na conta para atualizar a sessão.");
    }

    private ResultadoComandoLocal Falha(IdentityResult resultado)
    {
        logger.LogWarning("Atribuição local recusada pelo Identity: {Codigos}.", string.Join(", ", resultado.Errors.Select(e => e.Code)));
        return new(1, "Não foi possível concluir a atribuição. Verifique a conta e tente novamente.");
    }
}
