using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Administracao;

public interface IConsultaScoresAdministrativos
{
    Task<PaginaScoresAdministrativosDto> ListarAsync(int pagina, CancellationToken cancellationToken = default);
    Task<RelatorioScoreAdministrativoDto?> ConsultarAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record ListarScoresAdministrativosDto(int Pagina = 1);
public sealed record ConsultarScoreAdministrativoDto(Guid Id);
public sealed record PaginaScoresAdministrativosDto(int Pagina, int TamanhoPagina, int Total,
    IReadOnlyList<ItemScoreAdministrativoDto> Itens);
public sealed record ItemScoreAdministrativoDto(Guid Id, string AutorId, string? AutorEmail,
    string? NomeRebanho, string? Propriedade, DateTimeOffset CalculadoEmUtc,
    string VersaoPremissas, int Score, ClassificacaoRisco Classificacao);
public sealed record RelatorioScoreAdministrativoDto(Guid Id, string AutorId, string? AutorEmail,
    string? NomeRebanho, string? Propriedade, AnaliseCompletaDto Analise);
