using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using ScoreRebanho.Infrastructure.Clima;

namespace ScoreRebanho.Infrastructure.Tests;

[TestClass]
public sealed class OpenMeteoPrevisaoTests
{
    internal static string Resposta(int quantidade = 7, string unidadeVento = "km/h", decimal? chuva = 2m, int? codigoTempo = 95) => JsonSerializer.Serialize(new
    {
        timezone = "America/Sao_Paulo",
        daily_units = new Dictionary<string, string>
        {
            ["temperature_2m_min"] = "°C", ["temperature_2m_max"] = "°C",
            ["precipitation_sum"] = "mm", ["wind_speed_10m_max"] = unidadeVento, ["weather_code"] = "wmo code"
        },
        daily = new
        {
            time = Enumerable.Range(0, quantidade).Select(i => new DateOnly(2026, 11, 1).AddDays(i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            temperature_2m_min = Enumerable.Repeat(10m, quantidade), temperature_2m_max = Enumerable.Repeat(25m, quantidade),
            precipitation_sum = Enumerable.Repeat(chuva, quantidade), wind_speed_10m_max = Enumerable.Repeat(20m, quantidade),
            weather_code = Enumerable.Repeat(codigoTempo, quantidade)
        }
    });

    [TestMethod]
    public async Task PreservaSeteDiasUnidadesECoordenadasIndependentesDaCultura()
    {
        using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json(Resposta())));
        using var cliente = new HttpClient(handler) { BaseAddress = new("https://api.open-meteo.com/") };
        var relogio = new RelogioTeste();
        using var cache = new CachePrevisoes(relogio);
        var cultura = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            var resultado = await new OpenMeteoPrevisao(cliente, cache, relogio, NullLogger<OpenMeteoPrevisao>.Instance).ConsultarAsync(-27.81m, -50.32m);
            Assert.IsNotNull(resultado);
            Assert.HasCount(7, resultado.Dias);
            Assert.AreEqual(10m, resultado.Dias[0].TemperaturaMinimaCelsius);
            Assert.AreEqual(2m, resultado.Dias[0].PrecipitacaoMilimetros);
            Assert.AreEqual(20m, resultado.Dias[0].VentoMaximoQuilometrosHora);
            Assert.AreEqual(95, resultado.Dias[0].CodigoTempo);
            Assert.AreEqual(relogio.GetUtcNow(), resultado.ConsultadaEmUtc);
            StringAssert.Contains(handler.UltimaUrl!.Query, "latitude=-27.81");
            StringAssert.Contains(handler.UltimaUrl.Query, "forecast_days=7");
            StringAssert.Contains(handler.UltimaUrl.Query, "wind_speed_unit=kmh");
            StringAssert.Contains(handler.UltimaUrl.Query, "weather_code");
        }
        finally { CultureInfo.CurrentCulture = cultura; }
    }

    [TestMethod]
    public async Task CacheReutilizaAteTrintaMinutosETrocaDePontoConsultaNovamente()
    {
        using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json(Resposta())));
        using var cliente = new HttpClient(handler) { BaseAddress = new("https://api.open-meteo.com/") };
        var relogio = new RelogioTeste();
        using var cache = new CachePrevisoes(relogio);
        var consulta = new OpenMeteoPrevisao(cliente, cache, relogio, NullLogger<OpenMeteoPrevisao>.Instance);
        var original = await consulta.ConsultarAsync(-27m, -49m);
        relogio.Avancar(TimeSpan.FromMinutes(29));
        Assert.AreSame(original, await consulta.ConsultarAsync(-27m, -49m));
        Assert.AreEqual(1, handler.Chamadas);
        relogio.Avancar(TimeSpan.FromMinutes(1));
        var renovada = await consulta.ConsultarAsync(-27m, -49m);
        Assert.AreEqual(2, handler.Chamadas);
        Assert.AreNotEqual(original!.ConsultadaEmUtc, renovada!.ConsultadaEmUtc);
        await consulta.ConsultarAsync(-28m, -49m);
        Assert.AreEqual(3, handler.Chamadas);
    }

    [TestMethod]
    public async Task RespostaIncompletaOuUnidadesInvalidasNaoEntramNoCache()
    {
        foreach (var json in new[] { Resposta(6), Resposta(unidadeVento: "mph"), Resposta(chuva: null), Resposta(chuva: -1), "{}", "{" })
        {
            using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json(json)));
            using var cliente = new HttpClient(handler) { BaseAddress = new("https://api.open-meteo.com/") };
            var relogio = new RelogioTeste();
            using var cache = new CachePrevisoes(relogio);
            var consulta = new OpenMeteoPrevisao(cliente, cache, relogio, NullLogger<OpenMeteoPrevisao>.Instance);
            Assert.IsNull(await consulta.ConsultarAsync(-27m, -49m));
            Assert.IsNull(await consulta.ConsultarAsync(-27m, -49m));
            Assert.AreEqual(2, handler.Chamadas);
        }
    }

    [TestMethod]
    public async Task FalhaHttpETempoLimiteRetornamIndisponivelMasCancelamentoPropaga()
    {
        foreach (var timeout in new[] { false, true })
        {
            using var handler = new ClienteHttpSimulado((_, _) => timeout
                ? Task.FromException<HttpResponseMessage>(new TaskCanceledException("Timeout simulado"))
                : Task.FromResult(ClienteHttpSimulado.Json("{}", HttpStatusCode.ServiceUnavailable)));
            using var cliente = new HttpClient(handler) { BaseAddress = new("https://api.open-meteo.com/") };
            var relogio = new RelogioTeste();
            using var cache = new CachePrevisoes(relogio);
            var consulta = new OpenMeteoPrevisao(cliente, cache, relogio, NullLogger<OpenMeteoPrevisao>.Instance);
            Assert.IsNull(await consulta.ConsultarAsync(-27m, -49m));
            using var cancelamento = new CancellationTokenSource();
            cancelamento.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => consulta.ConsultarAsync(-27m, -49m, cancelamento.Token));
        }
    }

    [TestMethod]
    public async Task CondicaoAusenteOuSemUnidadeValidaNaoInventaCeuLimpo()
    {
        foreach (var alterar in new Action<System.Text.Json.Nodes.JsonNode>[]
        {
            json => json["daily"]!.AsObject().Remove("weather_code"),
            json => json["daily"]!["weather_code"] = new System.Text.Json.Nodes.JsonArray(95),
            json => json["daily_units"]!["weather_code"] = "desconhecida",
            json => json["daily"]!["weather_code"]![0] = null
        })
        {
            var json = System.Text.Json.Nodes.JsonNode.Parse(Resposta())!;
            alterar(json);
            using var handler = new ClienteHttpSimulado((_, _) => Task.FromResult(ClienteHttpSimulado.Json(json.ToJsonString())));
            using var cliente = new HttpClient(handler) { BaseAddress = new("https://api.open-meteo.com/") };
            var relogio = new RelogioTeste();
            using var cache = new CachePrevisoes(relogio);
            var resultado = await new OpenMeteoPrevisao(cliente, cache, relogio, NullLogger<OpenMeteoPrevisao>.Instance).ConsultarAsync(-27m, -49m);
            Assert.IsNotNull(resultado);
            Assert.HasCount(7, resultado.Dias);
            Assert.IsNull(resultado.Dias[0].CodigoTempo);
            Assert.AreEqual(10m, resultado.Dias[0].TemperaturaMinimaCelsius);
        }
    }

    private sealed class RelogioTeste : TimeProvider
    {
        private DateTimeOffset agora = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => agora;
        internal void Avancar(TimeSpan tempo) => agora += tempo;
    }
}
