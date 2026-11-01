using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Web.Comandos;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class AtribuirAcessoLocalTests
{
    [TestMethod]
    public async Task PerfilVigenteMudaRelatorioSemAlterarEntradasHistoricoOuOutrasPermissoes()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var rebanho = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var original = await FluxoRebanhosHttp.RebanhoAsync(app, rebanho, autor);
        var snapshot = JsonSerializer.Serialize(original);
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(autor);
        Assert.IsNotNull(usuario);
        var hash = usuario.PasswordHash;
        var executar = escopo.ServiceProvider.GetRequiredService<ExecutarComandoContaLocal>();
        var comando = new DefinirPerfilLocal(usuario.Email!, PerfisNegocio.Cooperativa);
        Assert.AreEqual(0, (await executar.ExecutarAsync(comando)).CodigoSaida);
        Assert.AreEqual(0, (await executar.ExecutarAsync(comando)).CodigoSaida);
        Assert.IsEmpty(await usuarios.GetRolesAsync(usuario));
        var claims = await usuarios.GetClaimsAsync(usuario);
        Assert.HasCount(1, claims.Where(c => c.Type == PerfisNegocio.TipoClaim));
        Assert.AreEqual(PerfisNegocio.Cooperativa, claims.Single(c => c.Type == PerfisNegocio.TipoClaim).Value);
        StringAssert.Contains(await cliente.GetStringAsync($"/Rebanhos/Detalhes/{rebanho}"), "id=\"componentes-score\"");
        StringAssert.Contains(await cliente.GetStringAsync($"/Historico/Detalhes/{original.UltimoScoreId}"), "id=\"componentes-score\"");
        Assert.AreEqual(0, (await executar.ExecutarAsync(new DefinirPerfilLocal(usuario.Email!, PerfisNegocio.Pecuarista))).CodigoSaida);
        Assert.DoesNotContain("id=\"componentes-score\"", await cliente.GetStringAsync($"/Historico/Detalhes/{original.UltimoScoreId}"));
        Assert.AreEqual(hash, (await usuarios.FindByIdAsync(autor))!.PasswordHash);
        Assert.AreEqual(snapshot, JsonSerializer.Serialize(await FluxoRebanhosHttp.RebanhoAsync(app, rebanho, autor)));
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task AdministradorEIndependenteDoPerfilEContinuaSemAcessoAoCrudAlheio(string perfil)
    {
        await using var app = new AplicacaoTeste();
        using var dono = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, dono);
        var rebanho = await FluxoRebanhosHttp.CriarRebanhoAsync(dono);
        using var operador = app.CriarCliente();
        var id = await FluxoRebanhosHttp.CriarContaAsync(app, operador, perfil);
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(id);
        Assert.IsNotNull(usuario);
        var executar = escopo.ServiceProvider.GetRequiredService<ExecutarComandoContaLocal>();
        var comando = new ConcederAdministradorLocal(usuario.Email!);
        Assert.AreEqual(0, (await executar.ExecutarAsync(comando)).CodigoSaida);
        Assert.AreEqual(0, (await executar.ExecutarAsync(comando)).CodigoSaida);
        Assert.HasCount(1, await usuarios.GetRolesAsync(usuario));
        Assert.IsTrue(await usuarios.IsInRoleAsync(usuario, "Administrador"));
        Assert.AreEqual(perfil, (await usuarios.GetClaimsAsync(usuario)).Single(c => c.Type == PerfisNegocio.TipoClaim).Value);
        Assert.AreEqual(HttpStatusCode.NotFound, (await operador.GetAsync($"/Rebanhos/Detalhes/{rebanho}")).StatusCode);
        var perfilAlternativo = perfil == PerfisNegocio.Pecuarista ? PerfisNegocio.Cooperativa : PerfisNegocio.Pecuarista;
        Assert.AreEqual(0, (await executar.ExecutarAsync(new DefinirPerfilLocal(usuario.Email!, perfilAlternativo))).CodigoSaida);
        Assert.IsTrue(await usuarios.IsInRoleAsync(usuario, "Administrador"));
    }

    [TestMethod]
    public async Task ContaInexistenteNaoCriaContaNemPapel()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await using var escopo = app.Services.CreateAsyncScope();
        var executar = escopo.ServiceProvider.GetRequiredService<ExecutarComandoContaLocal>();
        Assert.AreEqual(3, (await executar.ExecutarAsync(new DefinirPerfilLocal("ausente@example.com", PerfisNegocio.Cooperativa))).CodigoSaida);
        Assert.AreEqual(3, (await executar.ExecutarAsync(new ConcederAdministradorLocal("ausente@example.com"))).CodigoSaida);
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        Assert.IsNull(await usuarios.FindByEmailAsync("ausente@example.com"));
        Assert.IsFalse(await escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().RoleExistsAsync("Administrador"));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    public async Task PerfilAusenteOuAmbiguoERecusadoSemReparacaoImplicita(int quantidade)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var id = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(id);
        Assert.IsNotNull(usuario);
        if (quantidade == 0)
            Assert.IsTrue((await usuarios.RemoveClaimAsync(usuario, new Claim(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista))).Succeeded);
        else
            Assert.IsTrue((await usuarios.AddClaimAsync(usuario, new Claim(PerfisNegocio.TipoClaim, PerfisNegocio.Cooperativa))).Succeeded);
        var executar = escopo.ServiceProvider.GetRequiredService<ExecutarComandoContaLocal>();
        Assert.AreEqual(1, (await executar.ExecutarAsync(new DefinirPerfilLocal(usuario.Email!, PerfisNegocio.Cooperativa))).CodigoSaida);
        Assert.HasCount(quantidade, (await usuarios.GetClaimsAsync(usuario)).Where(c => c.Type == PerfisNegocio.TipoClaim));
    }
}
