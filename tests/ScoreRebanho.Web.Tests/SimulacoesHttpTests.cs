using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class SimulacoesHttpTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task GerarEditarRecalcularENovoScorePreservamHistorico(string perfil)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, perfil);
        var dados = Dados();
        dados["Entradas.AreaHectares"] = "100,5";
        dados["AutorId"] = "forjado";
        dados["PerfilNegocio"] = PerfisNegocio.Cooperativa;
        dados["Score"] = "100";
        var primeiro = await FluxoRebanhosHttp.PostAsync(cliente, "/Simulacoes/Gerar", dados, "/Simulacoes/Nova");
        Assert.AreEqual(HttpStatusCode.Redirect, primeiro.StatusCode, await primeiro.Content.ReadAsStringAsync());
        var id = Guid.Parse(primeiro.Headers.Location!.OriginalString.Split('/').Last());
        var pagina = await cliente.GetStringAsync(primeiro.Headers.Location);
        StringAssert.Contains(WebUtility.HtmlDecode(pagina), "Simulação avulsa");
        StringAssert.Contains(pagina, $"/Simulacoes/Editar/{id}");
        if (perfil == PerfisNegocio.Pecuarista)
        {
            Assert.DoesNotContain("id=\"componentes-score\"", pagina);
            Assert.DoesNotContain("id=\"custos-detalhados\"", pagina);
            Assert.DoesNotContain("id=\"premissas-detalhadas\"", pagina);
        }
        else StringAssert.Contains(pagina, "id=\"componentes-score\"");
        await using var escopo = app.Services.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioScores>();
        var original = await repositorio.ConsultarDoAutorAsync(id, autor);
        Assert.IsNotNull(original);
        Assert.IsNull(original.RebanhoOrigemId);
        Assert.AreEqual(100.5m, original.Analise.Entradas.AreaHectares);
        var snapshot = JsonSerializer.Serialize(original);
        var edicao = await cliente.GetStringAsync($"/Simulacoes/Editar/{id}");
        Assert.AreEqual("100,5", FluxoRebanhosHttp.Campo(edicao, "Entradas.AreaHectares"));
        Assert.AreEqual("180", FluxoRebanhosHttp.Campo(edicao, "Entradas.DiasPastagem"));
        Assert.DoesNotContain("name=\"Nome\"", edicao);
        Assert.DoesNotContain("name=\"Propriedade\"", edicao);
        dados["Entradas.DiasPastagem"] = "300";
        var segundo = await FluxoRebanhosHttp.PostAsync(cliente, "/Simulacoes/Gerar", dados, $"/Simulacoes/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.Redirect, segundo.StatusCode);
        Assert.AreNotEqual(primeiro.Headers.Location, segundo.Headers.Location);
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        Assert.AreEqual(snapshot, JsonSerializer.Serialize(await repositorio.ConsultarDoAutorAsync(id, autor)));
        var limpo = await cliente.GetStringAsync("/Simulacoes/Nova");
        Assert.AreEqual("", FluxoRebanhosHttp.Campo(limpo, "Entradas.AreaHectares"));
        Assert.AreEqual("180", FluxoRebanhosHttp.Campo(limpo, "Entradas.DiasPastagem"));
        StringAssert.Contains(WebUtility.HtmlDecode(await cliente.GetStringAsync("/Rebanhos")), "Você ainda");
    }

    [TestMethod]
    public async Task ConferenciaEEntradasInvalidasNaoSalvamEPreservamPreenchimento()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = Dados();
        var conferir = await FluxoRebanhosHttp.PostAsync(cliente, "/Simulacoes/Conferir", dados, "/Simulacoes/Nova");
        Assert.AreEqual(HttpStatusCode.OK, conferir.StatusCode);
        var html = WebUtility.HtmlDecode(await conferir.Content.ReadAsStringAsync());
        StringAssert.Contains(html, "Conferência da ocupação");
        Assert.DoesNotContain("Resumo financeiro", html);
        Assert.DoesNotContain("id=\"componentes-score\"", html);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        dados["Entradas.AreaHectares"] = "0";
        var invalido = await FluxoRebanhosHttp.PostAsync(cliente, "/Simulacoes/Gerar", dados, "/Simulacoes/Nova");
        Assert.AreEqual(HttpStatusCode.OK, invalido.StatusCode);
        var erros = await invalido.Content.ReadAsStringAsync();
        StringAssert.Contains(WebUtility.HtmlDecode(erros), "maior que zero");
        Assert.AreEqual("100", FluxoRebanhosHttp.Campo(erros, "Entradas.QuantidadeAnimais"));
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OutroAutorNaoAbreEdicaoOuRelatorioMesmoAdministrador(bool administrador)
    {
        await using var app = new AplicacaoTeste();
        using var dono = app.CriarCliente();
        var autorId = await FluxoRebanhosHttp.CriarContaAsync(app, dono);
        var salvo = await FluxoRebanhosHttp.PostAsync(dono, "/Simulacoes/Gerar", Dados(), "/Simulacoes/Nova");
        Assert.AreEqual(HttpStatusCode.Redirect, salvo.StatusCode);
        var id = Guid.Parse(salvo.Headers.Location!.OriginalString.Split('/').Last());
        using var outro = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, outro, PerfisNegocio.Cooperativa, administrador);
        Assert.AreEqual(HttpStatusCode.NotFound, (await outro.GetAsync($"/Simulacoes/Editar/{id}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await outro.GetAsync(salvo.Headers.Location)).StatusCode);
        var rebanho = await FluxoRebanhosHttp.CriarRebanhoAsync(dono);
        var cadastro = await FluxoRebanhosHttp.RebanhoAsync(app, rebanho, autorId);
        Assert.AreEqual(HttpStatusCode.NotFound, (await dono.GetAsync($"/Simulacoes/Editar/{cadastro.UltimoScoreId}")).StatusCode);
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task RotasExigemLoginEPostsExigemCsrf()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        foreach (var rota in new[] { "/Simulacoes/Nova", $"/Simulacoes/Editar/{Guid.NewGuid()}" })
        {
            var resposta = await cliente.GetAsync(rota);
            Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
            StringAssert.StartsWith(resposta.Headers.Location!.AbsolutePath, "/Conta/Entrar");
        }
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        foreach (var rota in new[] { "/Simulacoes/Gerar", "/Simulacoes/Conferir" })
            Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.PostAsync(rota, new FormUrlEncodedContent(Dados()))).StatusCode);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    private static Dictionary<string, string> Dados() => FluxoRebanhosHttp.Formulario()
        .Where(c => c.Key.StartsWith("Entradas.", StringComparison.Ordinal)).ToDictionary(c => c.Key, c => c.Value);
}
