using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Application.Rebanhos;

// Contrato interno de persistência; análises e autoria são produzidas pelo servidor.
public interface IRepositorioRebanhos
{
    Task<RebanhoGravadoDto> CriarAsync(string autorId, IdentificacaoRebanho identificacao,
        AnaliseCompletaDto analise, CancellationToken cancellationToken = default);
    Task<ResultadoGravacaoRebanho> AtualizarAsync(Guid id, string autorId, Guid versaoEsperada,
        IdentificacaoRebanho identificacao, AnaliseCompletaDto analise, CancellationToken cancellationToken = default);
    Task<EstadoGravacaoRebanho> ExcluirAsync(Guid id, string autorId, Guid versaoEsperada,
        CancellationToken cancellationToken = default);
    Task<RebanhoPersistidoDto?> ConsultarAsync(Guid id, string autorId, CancellationToken cancellationToken = default);
    Task<PaginaRebanhosDto> ListarAsync(string autorId, int pagina, CancellationToken cancellationToken = default);
}
