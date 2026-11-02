using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using ScoreRebanho.Infrastructure.Localizacao;

namespace ScoreRebanho.Infrastructure.Tests;

[TestClass]
public sealed class OpenMeteoMunicipiosTests
{
    [TestMethod]
    public async Task BuscaEscapaNomeEPreservaEscolhasComCoordenadasValidas()
    {
        using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json("""
            {"results":[{"name":"Lages","admin1":"Santa Catarina","country":"Brasil","latitude":-27.81,"longitude":-50.32},
            {"name":"Inválido","latitude":91,"longitude":0}]}
            """)));
        using var cliente = new HttpClient(handler) { BaseAddress = new("https://geocoding-api.open-meteo.com/") };
        var resultado = await new OpenMeteoMunicipios(cliente, NullLogger<OpenMeteoMunicipios>.Instance).BuscarAsync("Lages, SC & teste");
        Assert.IsNotNull(resultado);
        Assert.HasCount(1, resultado);
        Assert.AreEqual(-27.81m, resultado[0].Latitude);
        StringAssert.Contains(handler.UltimaUrl!.Query, "countryCode=BR");
        StringAssert.Contains(handler.UltimaUrl.Query, "%26");
    }

    [TestMethod]
    [DataRow("{}", 200, false)]
    [DataRow("{", 200, true)]
    [DataRow("{}", 503, true)]
    public async Task DistingueSemResultadosDeIndisponibilidade(string corpo, int status, bool indisponivel)
    {
        using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json(corpo, (HttpStatusCode)status)));
        using var cliente = new HttpClient(handler) { BaseAddress = new("https://geocoding-api.open-meteo.com/") };
        var resultado = await new OpenMeteoMunicipios(cliente, NullLogger<OpenMeteoMunicipios>.Instance).BuscarAsync("Teste");
        Assert.AreEqual(indisponivel, resultado is null);
        if (!indisponivel) Assert.HasCount(0, resultado!);
    }
}
