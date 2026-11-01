using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class AdministracaoScoresHttpTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task AdministradorAbreHistoricoAlheioCompletoSemAlterarCrudOuSnapshot(string perfil)
    {
        await using var app = new AplicacaoTeste();
        using var dono = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, dono);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(dono, "Histórico original");
        var original = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        var snapshot = JsonSerializer.Serialize(original.Analise);
        using var admin = app.CriarCliente();
        await AdministradorAsync(app, admin, perfil);
        var lista = await admin.GetAsync("/AdministracaoScores");
        Assert.AreEqual(HttpStatusCode.OK, lista.StatusCode);
        Assert.IsTrue(lista.Headers.CacheControl?.NoStore);
        StringAssert.Contains(WebUtility.HtmlDecode(await lista.Content.ReadAsStringAsync()), "Histórico original");
        var caminho = $"/AdministracaoScores/Detalhes/{original.UltimoScoreId}";
        var resposta = await admin.GetAsync(caminho);
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        var html = await resposta.Content.ReadAsStringAsync();
        foreach (var secao in new[] { "componentes-score", "custos-detalhados", "entradas-consolidadas", "premissas-detalhadas", "memoria-calculo" })
            StringAssert.Contains(html, $"id=\"{secao}\"");
        StringAssert.Contains(html, autor);
        Assert.DoesNotContain("/Rebanhos/Editar", html);
        Assert.DoesNotContain("/Simulacoes/Editar", html);
        Assert.AreEqual(HttpStatusCode.NotFound, (await admin.GetAsync($"/Rebanhos/Detalhes/{id}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await admin.GetAsync($"/Historico/Detalhes/{original.UltimoScoreId}")).StatusCode);
        foreach (var acao in new[] { "Index", "Detalhes", "Editar", "Excluir" })
        {
            var post = await admin.PostAsync($"/AdministracaoScores/{acao}/{original.UltimoScoreId}", new FormUrlEncodedContent(new Dictionary<string, string>()));
            Assert.IsTrue(post.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        }
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        var confirmacao = await dono.GetStringAsync($"/Rebanhos/Excluir/{id}");
        await FluxoRebanhosHttp.PostAsync(dono, $"/Rebanhos/Excluir/{id}", new()
        {
            ["Id"] = id.ToString(), ["Versao"] = FluxoRebanhosHttp.Campo(confirmacao, "Versao")
        }, $"/Rebanhos/Excluir/{id}");
        Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync(caminho)).StatusCode);
        await using var escopo = app.Services.CreateAsyncScope();
        var historico = await escopo.ServiceProvider.GetRequiredService<IRepositorioScores>().ConsultarDoAutorAsync(original.UltimoScoreId, autor);
        Assert.IsNotNull(historico);
        Assert.AreEqual(snapshot, JsonSerializer.Serialize(historico.Analise));
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task PerfilSemPapelAdministrativoNaoAcessaNemHistoricoProprioPelaRotaGlobal(string perfil)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, perfil);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var cadastro = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        foreach (var caminho in new[] { "/AdministracaoScores?Role=Administrador", $"/AdministracaoScores/Detalhes/{cadastro.UltimoScoreId}?perfil=Administrador" })
        {
            var resposta = await cliente.GetAsync(caminho);
            Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
            StringAssert.StartsWith(resposta.Headers.Location!.AbsolutePath, "/Conta/AcessoNegado");
            Assert.AreEqual(HttpStatusCode.Forbidden, (await cliente.GetAsync(resposta.Headers.Location)).StatusCode);
        }
        Assert.DoesNotContain("/AdministracaoScores", await cliente.GetStringAsync("/Rebanhos"));
    }

    [TestMethod]
    public async Task RemocaoDoPapelNoStoreRecusaCookieAdministrativoAntigo()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var id = await AdministradorAsync(app, cliente);
        Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync("/AdministracaoScores")).StatusCode);
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(id);
        Assert.IsNotNull(usuario);
        Assert.IsTrue((await usuarios.RemoveFromRoleAsync(usuario, PapeisContas.Administrador)).Succeeded);
        foreach (var rota in new[] { "/AdministracaoScores", $"/AdministracaoScores/Detalhes/{Guid.NewGuid()}" })
        {
            var resposta = await cliente.GetAsync(rota);
            Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
            StringAssert.StartsWith(resposta.Headers.Location!.AbsolutePath, "/Conta/AcessoNegado");
        }
    }

    [TestMethod]
    public async Task ListagemPaginaEmVinteOrdenaPorDataERecusaPaginasInvalidas()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await AdministradorAsync(app, cliente);
        StringAssert.Contains(await cliente.GetStringAsync("/AdministracaoScores"), "Nenhum score");
        var ids = new List<Guid>();
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var calcular = escopo.ServiceProvider.GetRequiredService<CalcularScoreRebanho>();
            var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioScores>();
            var analise = calcular.Executar(new()
            {
                AreaHectares = 100, Pastagem = ScoreRebanho.Domain.Pastagens.TipoPastagem.Verao,
                Posse = ScoreRebanho.Domain.Simulacoes.TipoPosse.Proprio, PastoPronto = true,
                QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10, DiasPastagem = 180
            }).Value;
            for (var i = 0; i < 21; i++)
                ids.Add(await repositorio.SalvarAvulsoAsync(autor, analise with { CalculadaEmUtc = DateTimeOffset.UnixEpoch.AddDays(i) }));
        }
        var primeira = await cliente.GetStringAsync("/AdministracaoScores");
        var segunda = await cliente.GetStringAsync("/AdministracaoScores?pagina=2");
        Assert.HasCount(20, Regex.Matches(primeira, "data-score-id="));
        Assert.HasCount(1, Regex.Matches(segunda, "data-score-id="));
        CollectionAssert.AreEqual(ids.AsEnumerable().Reverse().Take(20).Select(i => i.ToString()).ToArray(),
            Regex.Matches(primeira, "data-score-id=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToArray());
        StringAssert.Contains(segunda, ids[0].ToString());
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/AdministracaoScores?pagina=0")).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/AdministracaoScores?pagina=abc")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync($"/AdministracaoScores/Detalhes/{Guid.NewGuid()}")).StatusCode);
    }

    [TestMethod]
    public async Task RotasAdministrativasExigemLogin()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        foreach (var rota in new[] { "/AdministracaoScores", $"/AdministracaoScores/Detalhes/{Guid.NewGuid()}" })
        {
            var resposta = await cliente.GetAsync(rota);
            Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
            StringAssert.StartsWith(resposta.Headers.Location!.AbsolutePath, "/Conta/Entrar");
        }
    }

    private static async Task<string> AdministradorAsync(AplicacaoTeste app, HttpClient cliente, string perfil = PerfisNegocio.Pecuarista)
    {
        var id = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, perfil, administrador: true);
        string email;
        await using (var escopo = app.Services.CreateAsyncScope())
            email = (await escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>().FindByIdAsync(id))!.Email!;
        await FluxoRebanhosHttp.PostAsync(cliente, "/Conta/Sair", new(), "/Rebanhos");
        var login = await FluxoRebanhosHttp.PostAsync(cliente, "/Conta/Entrar", new()
        {
            ["Email"] = email, ["Senha"] = "Senha!123"
        }, "/Conta/Entrar");
        Assert.AreEqual(HttpStatusCode.Redirect, login.StatusCode);
        return id;
    }
}
