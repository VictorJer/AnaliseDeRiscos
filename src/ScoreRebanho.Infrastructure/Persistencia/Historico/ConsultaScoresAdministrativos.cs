using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Infrastructure.Persistencia.Historico;

public sealed class ConsultaScoresAdministrativos(ContasDbContext contexto) : IConsultaScoresAdministrativos
{
    private const int TamanhoPagina = 20;

    public async Task<PaginaScoresAdministrativosDto> ListarAsync(int pagina, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pagina);
        var scores = contexto.Set<ScoreHistoricoRegistro>().AsNoTracking();
        var total = await scores.CountAsync(cancellationToken);
        var deslocamento = ((long)pagina - 1) * TamanhoPagina;
        if (deslocamento > int.MaxValue) return new(pagina, TamanhoPagina, total, []);
        var itens = await (from score in scores
            join usuario in contexto.Users.AsNoTracking() on score.AutorId equals usuario.Id
            orderby score.CalculadoEmUtc descending, score.Id descending
            select new ItemScoreAdministrativoDto(score.Id, score.AutorId, usuario.Email,
                score.NomeRebanho, score.Propriedade, score.CalculadoEmUtc,
                score.VersaoPremissas, score.ScoreTotal, (ClassificacaoRisco)score.Classificacao))
            .Skip((int)deslocamento).Take(TamanhoPagina).ToArrayAsync(cancellationToken);
        return new(pagina, TamanhoPagina, total, Array.AsReadOnly(itens));
    }

    public async Task<RelatorioScoreAdministrativoDto?> ConsultarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registro = await (from score in contexto.Set<ScoreHistoricoRegistro>().AsNoTracking()
            join usuario in contexto.Users.AsNoTracking() on score.AutorId equals usuario.Id
            where score.Id == id
            select new { Score = score, AutorEmail = usuario.Email }).SingleOrDefaultAsync(cancellationToken);
        if (registro is null) return null;
        var historico = registro.Score;
        return new(historico.Id, historico.AutorId, registro.AutorEmail,
            historico.NomeRebanho, historico.Propriedade, historico.LerAnalise());
    }
}
