using FluentResults;

namespace ScoreRebanho.Application.Localizacao;

public sealed record BuscarMunicipiosDto(string Nome);
public sealed record MunicipioDto(string Nome, string Estado, string Pais, decimal Latitude, decimal Longitude);

public interface IConsultaMunicipios
{
    Task<IReadOnlyList<MunicipioDto>?> BuscarAsync(string nome, CancellationToken cancellationToken = default);
}

public sealed class BuscarMunicipios(IConsultaMunicipios consulta)
{
    public async Task<Result<IReadOnlyList<MunicipioDto>>> ExecutarAsync(BuscarMunicipiosDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var nome = entrada.Nome?.Trim() ?? "";
        if (nome.Length is < 3 or > 100)
            return Result.Fail<IReadOnlyList<MunicipioDto>>("Informe um município com 3 a 100 caracteres.");
        var municipios = await consulta.BuscarAsync(nome, cancellationToken);
        return municipios is null
            ? Result.Fail<IReadOnlyList<MunicipioDto>>("A busca de municípios está indisponível. Tente novamente ou selecione um ponto no mapa.")
            : Result.Ok(municipios);
    }
}
