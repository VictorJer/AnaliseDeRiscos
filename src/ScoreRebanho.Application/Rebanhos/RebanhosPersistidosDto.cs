using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Rebanhos;

public enum EstadoGravacaoRebanho { Sucesso, NaoEncontrado, Conflito }

public sealed record RebanhoGravadoDto(Guid Id, Guid Versao, Guid UltimoScoreId);

public sealed record ResultadoGravacaoRebanho(EstadoGravacaoRebanho Estado, RebanhoGravadoDto? Rebanho);

public sealed record RebanhoPersistidoDto(Guid Id, string Nome, string Propriedade, Guid Versao,
    Guid UltimoScoreId, DateTimeOffset CriadoEmUtc, DateTimeOffset AtualizadoEmUtc, AnaliseCompletaDto Analise, decimal? Latitude = null, decimal? Longitude = null);

public sealed record ItemListaRebanhosDto(Guid Id, string Nome, string Propriedade,
    DateTimeOffset AtualizadoEmUtc, int Score, ClassificacaoRisco Classificacao, decimal? Latitude = null, decimal? Longitude = null);

public sealed record PaginaRebanhosDto(int Pagina, int TamanhoPagina, int Total,
    IReadOnlyList<ItemListaRebanhosDto> Itens);
