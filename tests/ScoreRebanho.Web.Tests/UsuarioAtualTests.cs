using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Relatorios;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class UsuarioAtualTests
{
    [TestMethod]
    public async Task PerfilVigenteDoStoreSubstituiClaimAntigoDoCookie()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await CriarUsuarioAsync(usuarios);
        var contexto = escopo.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        contexto.HttpContext = new DefaultHttpContext { User = Principal(usuario.Id, PerfisNegocio.Cooperativa) };
        try
        {
            var atual = escopo.ServiceProvider.GetRequiredService<IUsuarioAtual>();
            Assert.AreEqual(PerfisNegocio.Pecuarista, (await atual.ObterAsync())!.PerfilNegocio);
            var criado = (await escopo.ServiceProvider.GetRequiredService<CriarRebanho>().ExecutarAsync(Entrada())).Value;
            var consultar = escopo.ServiceProvider.GetRequiredService<ConsultarRebanho>();
            Assert.IsInstanceOfType<ResumoPecuaristaDto>((await consultar.ExecutarAsync(new(criado.Id))).Value.Relatorio);
            Assert.IsTrue((await usuarios.ReplaceClaimAsync(usuario,
                new(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista),
                new(PerfisNegocio.TipoClaim, PerfisNegocio.Cooperativa))).Succeeded);
            Assert.IsInstanceOfType<RelatorioCooperativaDto>((await consultar.ExecutarAsync(new(criado.Id))).Value.Relatorio);
            Assert.IsTrue((await usuarios.ReplaceClaimAsync(usuario,
                new(PerfisNegocio.TipoClaim, PerfisNegocio.Cooperativa),
                new(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista))).Succeeded);
            Assert.IsInstanceOfType<ResumoPecuaristaDto>((await consultar.ExecutarAsync(new(criado.Id))).Value.Relatorio);
        }
        finally
        {
            contexto.HttpContext = null;
        }
    }

    [TestMethod]
    public async Task AdministradorContinuaComPerfilPecuaristaESemAcessoAoRebanhoDeOutroAutor()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var administrador = await CriarUsuarioAsync(usuarios);
        var dono = await CriarUsuarioAsync(usuarios);
        var papeis = escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.IsTrue((await papeis.CreateAsync(new("Administrador"))).Succeeded);
        Assert.IsTrue((await usuarios.AddToRoleAsync(administrador, "Administrador")).Succeeded);
        var contexto = escopo.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        contexto.HttpContext = new DefaultHttpContext { User = Principal(dono.Id) };
        try
        {
            var criar = escopo.ServiceProvider.GetRequiredService<CriarRebanho>();
            var criado = (await criar.ExecutarAsync(Entrada())).Value;
            contexto.HttpContext.User = Principal(administrador.Id, PerfisNegocio.Cooperativa, administrador: true);
            var consulta = await escopo.ServiceProvider.GetRequiredService<ConsultarRebanho>().ExecutarAsync(new(criado.Id));
            Assert.IsTrue(consulta.IsFailed);
            Assert.AreEqual("NAO_ENCONTRADO", consulta.Errors[0].Metadata["Codigo"]);
            var editado = await escopo.ServiceProvider.GetRequiredService<AtualizarRebanho>()
                .ExecutarAsync(new(criado.Id, criado.Versao, "Tentativa", "Fazenda", Entrada().Entradas));
            Assert.IsTrue(editado.IsFailed);
            Assert.AreEqual("NAO_ENCONTRADO", editado.Errors[0].Metadata["Codigo"]);
            var exclusao = await escopo.ServiceProvider.GetRequiredService<ExcluirRebanho>().ExecutarAsync(new(criado.Id, criado.Versao));
            Assert.IsTrue(exclusao.IsFailed);
            Assert.AreEqual("NAO_ENCONTRADO", exclusao.Errors[0].Metadata["Codigo"]);
            var proprio = (await criar.ExecutarAsync(Entrada())).Value;
            Assert.IsInstanceOfType<ResumoPecuaristaDto>(
                (await escopo.ServiceProvider.GetRequiredService<ConsultarRebanho>().ExecutarAsync(new(proprio.Id))).Value.Relatorio);
        }
        finally
        {
            contexto.HttpContext = null;
        }
    }

    [TestMethod]
    public async Task ClaimsDePerfilAmbiguosNoStoreNaoConcedemRelatorioCompleto()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await CriarUsuarioAsync(usuarios);
        Assert.IsTrue((await usuarios.AddClaimAsync(usuario, new(PerfisNegocio.TipoClaim, PerfisNegocio.Cooperativa))).Succeeded);
        var contexto = escopo.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        contexto.HttpContext = new DefaultHttpContext { User = Principal(usuario.Id) };
        try
        {
            var resultado = await escopo.ServiceProvider.GetRequiredService<ListarRebanhos>().ExecutarAsync(new());
            Assert.IsTrue(resultado.IsFailed);
            Assert.AreEqual("PERFIL_INVALIDO", resultado.Errors[0].Metadata["Codigo"]);
        }
        finally
        {
            contexto.HttpContext = null;
        }
    }

    [TestMethod]
    public async Task IdentidadeNaoAutenticadaOuContaInexistenteNaoEUsuarioAtual()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await using var escopo = app.Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        contexto.HttpContext = new DefaultHttpContext();
        try
        {
            var atual = escopo.ServiceProvider.GetRequiredService<IUsuarioAtual>();
            Assert.IsNull(await atual.ObterAsync());
            contexto.HttpContext.User = Principal("id-inexistente", PerfisNegocio.Cooperativa);
            Assert.IsNull(await atual.ObterAsync());
        }
        finally
        {
            contexto.HttpContext = null;
        }
    }

    private static async Task<IdentityUser> CriarUsuarioAsync(UserManager<IdentityUser> usuarios)
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var usuario = new IdentityUser { UserName = email, Email = email };
        Assert.IsTrue((await usuarios.CreateAsync(usuario, "Senha!123")).Succeeded);
        Assert.IsTrue((await usuarios.AddClaimAsync(usuario, new(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista))).Succeeded);
        return usuario;
    }

    private static ClaimsPrincipal Principal(string id, string perfil = PerfisNegocio.Pecuarista, bool administrador = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, id), new(PerfisNegocio.TipoClaim, perfil) };
        if (administrador)
            claims.Add(new(ClaimTypes.Role, "Administrador"));
        return new(new ClaimsIdentity(claims, "Teste"));
    }

    private static CriarRebanhoDto Entrada() => new("Lote", "Fazenda", new EntradaSimulacaoDto
    {
        AreaHectares = 100, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
        PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10,
        DiasPastagem = 180, VacinaCarbunculo = true, ControleCarrapato = true, Vermifugo = true
    });
}
