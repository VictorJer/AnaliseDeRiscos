using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Historico;

public interface IRepositorioScores
{
    Task<Guid> SalvarAvulsoAsync(string autorId, AnaliseCompletaDto analise, CancellationToken cancellationToken = default);
    Task<ScoreHistoricoDto?> ConsultarDoAutorAsync(Guid id, string autorId, CancellationToken cancellationToken = default);
}

public sealed record ScoreHistoricoDto(Guid Id, string AutorId, Guid? RebanhoOrigemId,
    string? NomeRebanho, string? Propriedade, DateTimeOffset CalculadoEmUtc, AnaliseCompletaDto Analise);
