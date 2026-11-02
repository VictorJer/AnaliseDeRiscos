using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ScoreRebanho.Application.Clima;

namespace ScoreRebanho.Infrastructure.Clima;

public sealed class OpenMeteoPrevisao(HttpClient cliente, CachePrevisoes cache, TimeProvider relogio,
    ILogger<OpenMeteoPrevisao> logger) : IConsultaPrevisao
{
    public async Task<PrevisaoDto?> ConsultarAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (cache.Obter(latitude, longitude) is { } salva) return salva;
        try
        {
            var url = FormattableString.Invariant($"v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_min,temperature_2m_max,precipitation_sum,wind_speed_10m_max&forecast_days=7&temperature_unit=celsius&precipitation_unit=mm&wind_speed_unit=kmh&timezone=auto");
            var resposta = await cliente.GetFromJsonAsync<Resposta>(url, cancellationToken);
            var previsao = Converter(resposta);
            if (previsao is not null) cache.Salvar(latitude, longitude, previsao);
            else logger.LogWarning("A fonte meteorológica devolveu dados incompletos ou unidades inesperadas.");
            return previsao;
        }
        catch (Exception erro) when (erro is HttpRequestException or JsonException or NotSupportedException
            || erro is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(erro, "Consulta meteorológica indisponível.");
            return null;
        }
    }

    private PrevisaoDto? Converter(Resposta? resposta)
    {
        if (resposta?.Daily is not { } dados || string.IsNullOrWhiteSpace(resposta.Timezone)) return null;
        var unidades = resposta.Units;
        if (unidades?.GetValueOrDefault("temperature_2m_min") != "°C"
            || unidades.GetValueOrDefault("temperature_2m_max") != "°C"
            || unidades.GetValueOrDefault("precipitation_sum") != "mm"
            || unidades.GetValueOrDefault("wind_speed_10m_max") != "km/h") return null;
        if (dados.Time?.Length != 7 || dados.Min?.Length != 7 || dados.Max?.Length != 7
            || dados.Chuva?.Length != 7 || dados.Vento?.Length != 7) return null;
        var dias = new List<DiaPrevisaoDto>(7);
        for (var i = 0; i < 7; i++)
        {
            if (!DateOnly.TryParseExact(dados.Time[i], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var data)
                || i > 0 && data.DayNumber != dias[i - 1].Data.DayNumber + 1
                || dados.Min[i] is not { } minima || dados.Max[i] is not { } maxima || minima > maxima
                || dados.Chuva[i] is not { } chuva || chuva < 0
                || dados.Vento[i] is not { } vento || vento < 0) return null;
            dias.Add(new(data, minima, maxima, chuva, vento));
        }
        return new(relogio.GetUtcNow(), resposta.Timezone, dias.AsReadOnly());
    }

    private sealed record Resposta(string? Timezone,
        [property: JsonPropertyName("daily_units")] Dictionary<string, string>? Units, DadosDiarios? Daily);
    private sealed record DadosDiarios(string?[]? Time,
        [property: JsonPropertyName("temperature_2m_min")] decimal?[]? Min,
        [property: JsonPropertyName("temperature_2m_max")] decimal?[]? Max,
        [property: JsonPropertyName("precipitation_sum")] decimal?[]? Chuva,
        [property: JsonPropertyName("wind_speed_10m_max")] decimal?[]? Vento);
}
