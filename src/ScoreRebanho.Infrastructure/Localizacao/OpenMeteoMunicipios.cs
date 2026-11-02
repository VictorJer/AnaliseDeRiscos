using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ScoreRebanho.Application.Localizacao;

namespace ScoreRebanho.Infrastructure.Localizacao;

public sealed class OpenMeteoMunicipios(HttpClient cliente, ILogger<OpenMeteoMunicipios> logger) : IConsultaMunicipios
{
    public async Task<IReadOnlyList<MunicipioDto>?> BuscarAsync(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            var resposta = await cliente.GetFromJsonAsync<Resposta>(
                $"v1/search?name={Uri.EscapeDataString(nome)}&count=10&language=pt&countryCode=BR", cancellationToken);
            if (resposta is null) return null;
            var municipios = new List<MunicipioDto>();
            foreach (var item in resposta.Results ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Name) || item.Latitude is null || item.Longitude is null)
                    continue;
                if (item.Latitude is < -90m or > 90m || item.Longitude is < -180m or > 180m)
                    continue;
                municipios.Add(new(item.Name, item.Admin1 ?? "", item.Country ?? "Brasil", item.Latitude.Value, item.Longitude.Value));
            }
            return municipios.AsReadOnly();
        }
        catch (Exception erro) when (erro is HttpRequestException or JsonException or NotSupportedException
            || erro is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(erro, "Busca de municípios indisponível.");
            return null;
        }
    }

    private sealed record Resposta([property: JsonPropertyName("results")] Item[]? Results);
    private sealed record Item(string? Name, string? Admin1, string? Country, decimal? Latitude, decimal? Longitude);
}
