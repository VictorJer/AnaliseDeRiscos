using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScoreRebanho.Application.Localizacao;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class MapaHttpTests
{
    [TestMethod]
    public async Task BuscaExigeLoginValidaEntradaEDevolveMunicipiosSemGravar()
    {
        var consulta = new MunicipiosTeste();
        await using var app = new AplicacaoTeste { ConfigurarServicos = services =>
        {
            services.RemoveAll<IConsultaMunicipios>();
            services.AddSingleton<IConsultaMunicipios>(consulta);
        }};
        using var cliente = app.CriarCliente();
        Assert.AreEqual(HttpStatusCode.Redirect, (await cliente.GetAsync("/Localizacao/Municipios?nome=Lages")).StatusCode);
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/Localizacao/Municipios?nome=La")).StatusCode);
        Assert.AreEqual(0, consulta.Chamadas);
        var resposta = await cliente.GetAsync("/Localizacao/Municipios?nome=Lages");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        StringAssert.Contains(await resposta.Content.ReadAsStringAsync(), "-27.81");
        Assert.AreEqual(1, consulta.Chamadas);
        Assert.AreEqual(0, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
    }

    [TestMethod]
    public async Task FormularioOfereceMapaERecursosLocaisSemDependerDeApiExterna()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var html = await cliente.GetStringAsync("/Rebanhos/Cadastrar");
        StringAssert.Contains(html, "data-localizacao");
        StringAssert.Contains(WebUtility.HtmlDecode(html), "Usar minha localização");
        foreach (var recurso in new[] { "/lib/leaflet/leaflet.js", "/lib/leaflet/leaflet.css", "/css/localizacao.css", "/js/localizacao.js" })
            Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync(recurso)).StatusCode, recurso);
    }

    private sealed class MunicipiosTeste : IConsultaMunicipios
    {
        internal int Chamadas { get; private set; }
        public Task<IReadOnlyList<MunicipioDto>?> BuscarAsync(string nome, CancellationToken cancellationToken = default)
        {
            Chamadas++;
            return Task.FromResult<IReadOnlyList<MunicipioDto>?>(new[] { new MunicipioDto("Lages", "Santa Catarina", "Brasil", -27.81m, -50.32m) });
        }
    }
}
