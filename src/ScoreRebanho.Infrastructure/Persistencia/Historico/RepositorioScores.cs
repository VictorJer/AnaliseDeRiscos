using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Infrastructure.Persistencia.Historico;

public sealed class RepositorioScores(ContasDbContext contexto) : IRepositorioScores
{
    public async Task<Guid> SalvarAvulsoAsync(string autorId, AnaliseCompletaDto analise, CancellationToken cancellationToken = default)
    {
        var score = ScoreHistoricoRegistro.Criar(Guid.NewGuid(), autorId, analise);
        contexto.Add(score);
        await contexto.SaveChangesAsync(cancellationToken);
        return score.Id;
    }

    public async Task<ScoreHistoricoDto?> ConsultarDoAutorAsync(Guid id, string autorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        var score = await contexto.Set<ScoreHistoricoRegistro>().AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == id && s.AutorId == autorId, cancellationToken);
        return score is null ? null : new(score.Id, score.AutorId, score.RebanhoOrigemId,
            score.NomeRebanho, score.Propriedade, score.CalculadoEmUtc, score.LerAnalise());
    }
}
