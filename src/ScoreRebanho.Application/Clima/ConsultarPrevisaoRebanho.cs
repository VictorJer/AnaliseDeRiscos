using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Rebanhos;

namespace ScoreRebanho.Application.Clima;

public sealed record ConsultarPrevisaoDto(Guid Id);
public sealed record DiaPrevisaoDto(DateOnly Data, decimal TemperaturaMinimaCelsius, decimal TemperaturaMaximaCelsius,
    decimal PrecipitacaoMilimetros, decimal VentoMaximoQuilometrosHora, int? CodigoTempo = null);
public sealed record PrevisaoDto(DateTimeOffset ConsultadaEmUtc, string FusoHorario, IReadOnlyList<DiaPrevisaoDto> Dias);
public sealed record PrevisaoRebanhoDto(Guid Id, string Nome, string Propriedade, decimal Latitude, decimal Longitude, PrevisaoDto Previsao);

public interface IConsultaPrevisao
{
    Task<PrevisaoDto?> ConsultarAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
}

public sealed class ConsultarPrevisaoRebanho(IUsuarioAtual usuarioAtual, IRepositorioRebanhos repositorio, IConsultaPrevisao consulta)
{
    private readonly AcessoRebanhos acesso = new(usuarioAtual, repositorio);

    public async Task<Result<PrevisaoRebanhoDto>> ExecutarAsync(ConsultarPrevisaoDto entrada, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var autorizado = await acesso.ConsultarAsync(entrada.Id, cancellationToken);
        if (autorizado.IsFailed) return Result.Fail<PrevisaoRebanhoDto>(autorizado.Errors);
        var rebanho = autorizado.Value.Rebanho;
        if (rebanho.Latitude is not { } latitude || rebanho.Longitude is not { } longitude)
            return Result.Fail<PrevisaoRebanhoDto>(new Error("Adicione a localização do rebanho para ver a previsão.").WithMetadata("Codigo", "SEM_LOCALIZACAO"));
        var previsao = await consulta.ConsultarAsync(latitude, longitude, cancellationToken);
        if (previsao is null)
            return Result.Fail<PrevisaoRebanhoDto>(new Error("A previsão está indisponível no momento. Tente novamente mais tarde.").WithMetadata("Codigo", "PREVISAO_INDISPONIVEL"));
        return Result.Ok(new PrevisaoRebanhoDto(rebanho.Id, rebanho.Nome, rebanho.Propriedade, latitude, longitude, previsao));
    }
}
