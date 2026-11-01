using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class RebanhosHttpTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task CrudCompletoUsaMesmosCamposEPreservaHistorico(string perfil)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, perfil);
        Assert.AreEqual("/Rebanhos", (await cliente.GetAsync("/")).Headers.Location?.OriginalString);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var pagina = await cliente.GetAsync($"/Rebanhos/Detalhes/{id}");
        var html = await pagina.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, pagina.StatusCode, html);
        StringAssert.Contains(WebUtility.HtmlDecode(html), "Lote serrano");
        VerificarPerfil(html, perfil);
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        var original = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        var edicao = await cliente.GetStringAsync($"/Rebanhos/Editar/{id}");
        Assert.AreEqual("Lote serrano", FluxoRebanhosHttp.Campo(edicao, "Nome"));
        Assert.AreEqual("100", FluxoRebanhosHttp.Campo(edicao, "Entradas.QuantidadeAnimais"));
        var dados = FluxoRebanhosHttp.Formulario("Lote atualizado");
        dados["Id"] = id.ToString();
        dados["Versao"] = FluxoRebanhosHttp.Campo(edicao, "Versao");
        dados["Entradas.DiasPastagem"] = "300";
        var atualizado = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Editar/{id}", dados, $"/Rebanhos/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.Redirect, atualizado.StatusCode);
        VerificarPerfil(await cliente.GetStringAsync($"/Rebanhos/Detalhes/{id}"), perfil);
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        var historico = await cliente.GetStringAsync($"/Historico/Detalhes/{original.UltimoScoreId}");
        VerificarPerfil(historico, perfil);
        StringAssert.Contains(WebUtility.HtmlDecode(historico), "Lote serrano");
        var confirmacao = await cliente.GetStringAsync($"/Rebanhos/Excluir/{id}");
        StringAssert.Contains(WebUtility.HtmlDecode(confirmacao), "Confirmar exclusão");
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        var excluido = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Excluir/{id}", new()
        {
            ["Id"] = id.ToString(), ["Versao"] = FluxoRebanhosHttp.Campo(confirmacao, "Versao")
        }, $"/Rebanhos/Excluir/{id}");
        Assert.AreEqual(HttpStatusCode.Redirect, excluido.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync($"/Rebanhos/Detalhes/{id}")).StatusCode);
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        VerificarPerfil(await cliente.GetStringAsync($"/Historico/Detalhes/{original.UltimoScoreId}"), perfil);
    }

    [TestMethod]
    public async Task ConferirOcupacaoNaoSalvaNemMostraScoreOuResultadoFinanceiro()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/ConferirCadastro",
            FluxoRebanhosHttp.Formulario(), "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        var html = await resposta.Content.ReadAsStringAsync();
        StringAssert.Contains(WebUtility.HtmlDecode(html), "Conferência da ocupação");
        Assert.DoesNotContain("Resumo financeiro", html);
        Assert.DoesNotContain("id=\"componentes-score\"", html);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        StringAssert.Contains(await cliente.GetStringAsync("/Rebanhos"), "Você ainda");
    }

    [TestMethod]
    public async Task ErrosPreservamFormularioEValoresDecimaisBrasileirosSaoAceitos()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = FluxoRebanhosHttp.Formulario("Nome preservado");
        dados["Entradas.AreaHectares"] = "0";
        var invalido = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.OK, invalido.StatusCode);
        var html = await invalido.Content.ReadAsStringAsync();
        Assert.AreEqual("Nome preservado", FluxoRebanhosHttp.Campo(html, "Nome"));
        StringAssert.Contains(WebUtility.HtmlDecode(html), "maior que zero");
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        dados["Entradas.AreaHectares"] = "100,5";
        dados["Entradas.PrecoCompraPorKgReais"] = "10,25";
        var valido = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.Redirect, valido.StatusCode);
        var id = Guid.Parse(valido.Headers.Location!.OriginalString.Split('/').Last());
        var salvo = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        Assert.AreEqual(100.5m, salvo.Analise.Entradas.AreaHectares);
        Assert.AreEqual(10.25m, salvo.Analise.Entradas.PrecoCompraPorKgReais);
    }

    [TestMethod]
    [DataRow("/Rebanhos")]
    [DataRow("/Rebanhos/Cadastrar")]
    [DataRow("/Rebanhos/Detalhes/11111111-1111-1111-1111-111111111111")]
    [DataRow("/Rebanhos/Editar/11111111-1111-1111-1111-111111111111")]
    [DataRow("/Rebanhos/Excluir/11111111-1111-1111-1111-111111111111")]
    [DataRow("/Historico/Detalhes/11111111-1111-1111-1111-111111111111")]
    public async Task RotasExigemLogin(string caminho)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var resposta = await cliente.GetAsync(caminho);
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        StringAssert.Contains(resposta.Headers.Location!.OriginalString, "/Conta/Entrar");
    }

    [TestMethod]
    [DataRow("/Rebanhos/Cadastrar")]
    [DataRow("/Rebanhos/ConferirCadastro")]
    [DataRow("/Rebanhos/Editar/11111111-1111-1111-1111-111111111111")]
    [DataRow("/Rebanhos/ConferirEdicao/11111111-1111-1111-1111-111111111111")]
    [DataRow("/Rebanhos/Excluir/11111111-1111-1111-1111-111111111111")]
    public async Task EscritasSemTokenCsrfSaoRecusadas(string caminho)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var resposta = await cliente.PostAsync(caminho, new FormUrlEncodedContent(FluxoRebanhosHttp.Formulario()));
        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista, false)]
    [DataRow(PerfisNegocio.Cooperativa, false)]
    [DataRow(PerfisNegocio.Pecuarista, true)]
    public async Task OutroAutorIncluindoAdministradorNaoAcessaEndpointsDiretos(string perfil, bool administrador)
    {
        await using var app = new AplicacaoTeste();
        using var dono = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, dono);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(dono, "Rebanho privado");
        var salvo = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        using var outro = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, outro, perfil, administrador);
        Assert.DoesNotContain("Rebanho privado", await outro.GetStringAsync("/Rebanhos"));
        foreach (var caminho in new[] { $"/Rebanhos/Detalhes/{id}", $"/Rebanhos/Editar/{id}", $"/Rebanhos/Excluir/{id}", $"/Historico/Detalhes/{salvo.UltimoScoreId}" })
            Assert.AreEqual(HttpStatusCode.NotFound, (await outro.GetAsync(caminho)).StatusCode);
        var dados = FluxoRebanhosHttp.Formulario();
        dados["Id"] = id.ToString(); dados["Versao"] = salvo.Versao.ToString();
        foreach (var acao in new[] { "Editar", "ConferirEdicao", "Excluir" })
            Assert.AreEqual(HttpStatusCode.NotFound,
                (await FluxoRebanhosHttp.PostAsync(outro, $"/Rebanhos/{acao}/{id}", dados, "/Rebanhos/Cadastrar")).StatusCode);
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task CamposIndevidosNaoAlteramAutoriaPerfilOuScore()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = FluxoRebanhosHttp.Formulario();
        dados["AutorId"] = "outro-usuario";
        dados["PerfilNegocio"] = "Cooperativa";
        dados["Score"] = "100";
        dados["Conferencia.OcupacaoDecimal"] = "0";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        var id = Guid.Parse(resposta.Headers.Location!.OriginalString.Split('/').Last());
        var salvo = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        Assert.IsNotNull(salvo);
        Assert.IsLessThan(100, salvo.Analise.Score.Total);
        VerificarPerfil(await cliente.GetStringAsync($"/Rebanhos/Detalhes/{id}?perfil=Cooperativa&completo=true"), PerfisNegocio.Pecuarista);
    }

    [TestMethod]
    public async Task VersaoObsoletaRetornaConflitoSemNovoHistorico()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var edicao = await cliente.GetStringAsync($"/Rebanhos/Editar/{id}");
        var dados = FluxoRebanhosHttp.Formulario("Atualizado");
        dados["Id"] = id.ToString();
        dados["Versao"] = FluxoRebanhosHttp.Campo(edicao, "Versao");
        Assert.AreEqual(HttpStatusCode.Redirect,
            (await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Editar/{id}", dados, $"/Rebanhos/Editar/{id}")).StatusCode);
        dados["Nome"] = "Preenchimento antigo";
        var conflito = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Editar/{id}", dados, $"/Rebanhos/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.Conflict, conflito.StatusCode);
        Assert.AreEqual("Preenchimento antigo", FluxoRebanhosHttp.Campo(await conflito.Content.ReadAsStringAsync(), "Nome"));
        Assert.AreEqual(HttpStatusCode.Conflict,
            (await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Excluir/{id}", dados, $"/Rebanhos/Excluir/{id}")).StatusCode);
        Assert.AreEqual(2, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task MudancaDePerfilReavaliaARespostaAtualEHistorica()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, PerfisNegocio.Cooperativa);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var salvo = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        VerificarPerfil(await cliente.GetStringAsync($"/Rebanhos/Detalhes/{id}"), PerfisNegocio.Cooperativa);
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var usuario = (await usuarios.FindByIdAsync(autor))!;
            Assert.IsTrue((await usuarios.ReplaceClaimAsync(usuario, new(PerfisNegocio.TipoClaim, PerfisNegocio.Cooperativa), new(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista))).Succeeded);
        }
        VerificarPerfil(await cliente.GetStringAsync($"/Rebanhos/Detalhes/{id}"), PerfisNegocio.Pecuarista);
        VerificarPerfil(await cliente.GetStringAsync($"/Historico/Detalhes/{salvo.UltimoScoreId}"), PerfisNegocio.Pecuarista);
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task ConferirEdicaoEConsultarNaoCriamHistorico()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var html = await cliente.GetStringAsync($"/Rebanhos/Editar/{id}");
        var dados = FluxoRebanhosHttp.Formulario();
        dados["Id"] = id.ToString();
        dados["Versao"] = FluxoRebanhosHttp.Campo(html, "Versao");
        dados["Entradas.QuantidadeAnimais"] = "120";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/ConferirEdicao/{id}", dados, $"/Rebanhos/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()), "Conferência da ocupação");
        await cliente.GetAsync($"/Rebanhos/Detalhes/{id}");
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task ListagemPaginaVintePorVezERespeitaOrdemDeAtualizacao()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var repositorio = escopo.ServiceProvider.GetRequiredService<ScoreRebanho.Application.Rebanhos.IRepositorioRebanhos>();
            var motor = escopo.ServiceProvider.GetRequiredService<ScoreRebanho.Application.Simulacoes.CalcularScoreRebanho>();
            var analise = motor.Executar(new()
            {
                AreaHectares = 100, Pastagem = ScoreRebanho.Domain.Pastagens.TipoPastagem.Verao,
                Posse = ScoreRebanho.Domain.Simulacoes.TipoPosse.Proprio, PastoPronto = true,
                QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10, DiasPastagem = 180
            }).Value;
            for (var indice = 0; indice < 21; indice++)
                await repositorio.CriarAsync(autor, new($"Cadastro {indice:D2}", "Fazenda"),
                    analise with { CalculadaEmUtc = DateTimeOffset.UnixEpoch.AddMinutes(indice) });
        }
        var primeira = await cliente.GetStringAsync("/Rebanhos");
        var segunda = await cliente.GetStringAsync("/Rebanhos?pagina=2");
        Assert.Contains("Cadastro 20", primeira);
        Assert.DoesNotContain("Cadastro 00", primeira);
        Assert.Contains("Cadastro 00", segunda);
        Assert.DoesNotContain("Cadastro 20", segunda);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/Rebanhos?pagina=0")).StatusCode);
    }

    [TestMethod]
    public async Task FormularioValidaCondicoesEArquivosBootstrapEstaoDisponiveis()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = FluxoRebanhosHttp.Formulario();
        dados["Entradas.Posse"] = "2";
        dados["Entradas.RotacaoAtiva"] = "true";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        var html = WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync());
        Assert.Contains("Informe o arrendamento mensal", html);
        Assert.Contains("quantidade positiva de piquetes", html);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync("/lib/bootstrap/dist/css/bootstrap.min.css")).StatusCode);
    }

    private static void VerificarPerfil(string html, string perfil)
    {
        foreach (var marcador in new[] { "componentes-score", "custos-detalhados", "produtos-detalhados", "entradas-consolidadas", "premissas-detalhadas", "memoria-calculo" })
            Assert.AreEqual(perfil == PerfisNegocio.Cooperativa, html.Contains($"id=\"{marcador}\""), marcador);
        if (perfil == PerfisNegocio.Pecuarista)
        {
            foreach (var detalhe in new[] { "GMD", "Mortalidade estimada", "DosePorAnimal", "AnaliseJson", "QuilogramasPorUa" })
                Assert.DoesNotContain(detalhe, html);
        }
    }
}
