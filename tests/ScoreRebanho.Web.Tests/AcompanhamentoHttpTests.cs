using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScoreRebanho.Application.Clima;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class AcompanhamentoHttpTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task CartoesNaoConsultamFonteEDetalhesUsamPontoSalvoSemAlterarHistorico(string perfil)
    {
        var fonte = new FonteTeste();
        await using var app = Aplicacao(fonte);
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente, perfil);
        var id = await CriarLocalizadoAsync(cliente);
        var original = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        var cartoes = WebUtility.HtmlDecode(await cliente.GetStringAsync("/Acompanhamento"));
        StringAssert.Contains(cartoes, "Ver previsão");
        StringAssert.Contains(cartoes, "Lote com localização");
        Assert.AreEqual(0, fonte.Chamadas);
        var resposta = await cliente.GetAsync($"/Acompanhamento/Detalhes/{id}?latitude=0&longitude=0&autorId=outro&perfil=Cooperativa");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        var html = WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync());
        Assert.HasCount(7, Regex.Matches(html, "data-dia="));
        StringAssert.Contains(html, "10,0 °C");
        StringAssert.Contains(html, "km/h");
        StringAssert.Contains(html, "Open-Meteo");
        StringAssert.Contains(html, "Tempestade");
        StringAssert.Contains(html, "Céu limpo");
        StringAssert.Contains(html, "icone-tempo");
        StringAssert.Contains(html, "aria-hidden=\"true\"");
        Assert.DoesNotContain("custos-detalhados", html);
        Assert.DoesNotContain("componentes-score", html);
        Assert.AreEqual(-27.81m, fonte.Latitude);
        Assert.AreEqual(-50.32m, fonte.Longitude);
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        Assert.AreEqual(original.Versao, (await FluxoRebanhosHttp.RebanhoAsync(app, id, autor)).Versao);
        Assert.IsTrue(resposta.Headers.CacheControl?.NoStore);
    }

    [TestMethod]
    public async Task AusenciaDeLocalizacaoOrientaEdicaoSemConsultarFonte()
    {
        var fonte = new FonteTeste();
        await using var app = Aplicacao(fonte);
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var id = await FluxoRebanhosHttp.CriarRebanhoAsync(cliente);
        var html = WebUtility.HtmlDecode(await cliente.GetStringAsync("/Acompanhamento"));
        StringAssert.Contains(html, "Adicionar localização para ver a previsão");
        StringAssert.Contains(html, "#localizacao");
        var semPonto = WebUtility.HtmlDecode(await cliente.GetStringAsync($"/Acompanhamento/Detalhes/{id}"));
        StringAssert.Contains(semPonto, "Adicione a localização");
        Assert.AreEqual(0, fonte.Chamadas);
    }

    [TestMethod]
    public async Task CooperativaAdministradoraNaoAbreLocalizacaoOuPrevisaoDeTerceiro()
    {
        var fonte = new FonteTeste();
        await using var app = Aplicacao(fonte);
        using var dono = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, dono);
        var id = await CriarLocalizadoAsync(dono);
        using var terceiro = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, terceiro, PerfisNegocio.Cooperativa, administrador: true);
        Assert.AreEqual(HttpStatusCode.NotFound, (await terceiro.GetAsync($"/Acompanhamento/Detalhes/{id}")).StatusCode);
        var lista = await terceiro.GetStringAsync("/Acompanhamento");
        Assert.DoesNotContain("Lote com local", lista);
        Assert.DoesNotContain(id.ToString(), lista);
        Assert.AreEqual(0, fonte.Chamadas);
    }

    [TestMethod]
    public async Task FalhaDaFonteMostraIndisponibilidadeSemDadosInventados()
    {
        var fonte = new FonteTeste { Indisponivel = true };
        await using var app = Aplicacao(fonte);
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var id = await CriarLocalizadoAsync(cliente);
        var resposta = await cliente.GetAsync($"/Acompanhamento/Detalhes/{id}");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        var html = WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync());
        StringAssert.Contains(html, "Tentar novamente");
        Assert.DoesNotContain("data-dia=", html);
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task ExigeLoginERecusaPaginaInvalida()
    {
        await using var app = Aplicacao(new());
        using var cliente = app.CriarCliente();
        Assert.AreEqual(HttpStatusCode.Redirect, (await cliente.GetAsync("/Acompanhamento")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, (await cliente.GetAsync($"/Acompanhamento/Detalhes/{Guid.NewGuid()}")).StatusCode);
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/Acompanhamento?pagina=0")).StatusCode);
    }

    private static AplicacaoTeste Aplicacao(FonteTeste fonte) => new()
    {
        ConfigurarServicos = servicos =>
        {
            servicos.RemoveAll<IConsultaPrevisao>();
            servicos.AddSingleton<IConsultaPrevisao>(fonte);
        }
    };

    private static async Task<Guid> CriarLocalizadoAsync(HttpClient cliente)
    {
        var dados = FluxoRebanhosHttp.Formulario("Lote com localização");
        dados["Latitude"] = "-27,81";
        dados["Longitude"] = "-50,32";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        return Guid.Parse(resposta.Headers.Location!.OriginalString.Split('/').Last());
    }

    private sealed class FonteTeste : IConsultaPrevisao
    {
        internal int Chamadas { get; private set; }
        internal decimal Latitude { get; private set; }
        internal decimal Longitude { get; private set; }
        internal bool Indisponivel { get; init; }
        public Task<PrevisaoDto?> ConsultarAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
        {
            Chamadas++; Latitude = latitude; Longitude = longitude;
            var dias = Enumerable.Range(0, 7).Select(i => new DiaPrevisaoDto(new DateOnly(2026, 11, 1).AddDays(i), 10, 25, 2, 20, i == 0 ? 95 : 0)).ToArray();
            return Task.FromResult(Indisponivel ? null : new PrevisaoDto(DateTimeOffset.UtcNow, "America/Sao_Paulo", Array.AsReadOnly(dias)));
        }
    }
}
