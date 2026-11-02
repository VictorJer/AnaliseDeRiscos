using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Rebanhos;
using ScoreRebanho.Domain.Scores;
using ScoreRebanho.Infrastructure.Persistencia.Historico;

namespace ScoreRebanho.Infrastructure.Persistencia.Rebanhos;

public sealed class RepositorioRebanhos(ContasDbContext contexto) : IRepositorioRebanhos
{
    private const int TamanhoPagina = 20;

    public async Task<RebanhoGravadoDto> CriarAsync(string autorId, IdentificacaoRebanho identificacao,
        AnaliseCompletaDto analise, CancellationToken cancellationToken = default, LocalizacaoRebanho? localizacao = null)
    {
        ArgumentNullException.ThrowIfNull(analise);
        var scoreId = Guid.NewGuid();
        var rebanho = Rebanho.Criar(autorId, identificacao, scoreId, analise.CalculadaEmUtc, localizacao);
        var score = ScoreHistoricoRegistro.Criar(scoreId, autorId, analise, rebanho.Id, identificacao);
        contexto.Add(score);
        contexto.Add(rebanho);
        // Um SaveChanges inclui as duas inclusões na mesma transação do EF Core.
        await contexto.SaveChangesAsync(cancellationToken);
        return Gravado(rebanho);
    }

    public async Task<ResultadoGravacaoRebanho> AtualizarAsync(Guid id, string autorId, Guid versaoEsperada,
        IdentificacaoRebanho identificacao, AnaliseCompletaDto analise, CancellationToken cancellationToken = default, LocalizacaoRebanho? localizacao = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentNullException.ThrowIfNull(identificacao);
        ArgumentNullException.ThrowIfNull(analise);
        var rebanho = await contexto.Set<Rebanho>().SingleOrDefaultAsync(r => r.Id == id && r.AutorId == autorId, cancellationToken);
        if (rebanho is null)
            return new(EstadoGravacaoRebanho.NaoEncontrado, null);
        if (rebanho.Versao != versaoEsperada)
            return new(EstadoGravacaoRebanho.Conflito, null);

        var identificacaoAnterior = rebanho.Identificacao;
        var score = ScoreHistoricoRegistro.Criar(Guid.NewGuid(), autorId, analise, rebanho.Id, identificacao);
        rebanho.Atualizar(identificacao, score.Id, analise.CalculadaEmUtc, localizacao);
        contexto.Add(score);
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
            return new(EstadoGravacaoRebanho.Sucesso, Gravado(rebanho));
        }
        catch (DbUpdateConcurrencyException)
        {
            Desanexar(rebanho, identificacaoAnterior, score);
            return new(EstadoGravacaoRebanho.Conflito, null);
        }
    }

    public async Task<EstadoGravacaoRebanho> ExcluirAsync(Guid id, string autorId, Guid versaoEsperada,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        var rebanho = await contexto.Set<Rebanho>().SingleOrDefaultAsync(r => r.Id == id && r.AutorId == autorId, cancellationToken);
        if (rebanho is null)
            return EstadoGravacaoRebanho.NaoEncontrado;
        if (rebanho.Versao != versaoEsperada)
            return EstadoGravacaoRebanho.Conflito;
        contexto.Remove(rebanho);
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
            return EstadoGravacaoRebanho.Sucesso;
        }
        catch (DbUpdateConcurrencyException)
        {
            Desanexar(rebanho, rebanho.Identificacao);
            return EstadoGravacaoRebanho.Conflito;
        }
    }

    public async Task<RebanhoPersistidoDto?> ConsultarAsync(Guid id, string autorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        var registro = await contexto.Set<Rebanho>().AsNoTracking()
            .Where(r => r.Id == id && r.AutorId == autorId)
            .Join(contexto.Set<ScoreHistoricoRegistro>().AsNoTracking(),
                r => new { Id = r.UltimoScoreId, r.AutorId }, s => new { s.Id, s.AutorId },
                (r, s) => new { Rebanho = r, Score = s })
            .SingleOrDefaultAsync(cancellationToken);
        if (registro is null)
            return null;
        var rebanho = registro.Rebanho;
        return new(rebanho.Id, rebanho.Identificacao.Nome, rebanho.Identificacao.Propriedade, rebanho.Versao,
            rebanho.UltimoScoreId, rebanho.CriadoEmUtc, rebanho.AtualizadoEmUtc, registro.Score.LerAnalise(), rebanho.Latitude, rebanho.Longitude);
    }

    public async Task<PaginaRebanhosDto> ListarAsync(string autorId, int pagina, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pagina);
        var consulta = contexto.Set<Rebanho>().AsNoTracking().Where(r => r.AutorId == autorId);
        var total = await consulta.CountAsync(cancellationToken);
        var inicio = (pagina - 1L) * TamanhoPagina;
        if (inicio >= total)
            return new(pagina, TamanhoPagina, total, Array.Empty<ItemListaRebanhosDto>());
        var itens = await consulta.OrderByDescending(r => r.AtualizadoEmUtc).ThenByDescending(r => r.Id)
            .Skip((int)inicio).Take(TamanhoPagina)
            .Join(contexto.Set<ScoreHistoricoRegistro>().AsNoTracking(),
                r => new { Id = r.UltimoScoreId, r.AutorId }, s => new { s.Id, s.AutorId },
                (r, s) => new ItemListaRebanhosDto(r.Id, r.Identificacao.Nome, r.Identificacao.Propriedade,
                    r.AtualizadoEmUtc, s.ScoreTotal, (ClassificacaoRisco)s.Classificacao, r.Latitude, r.Longitude))
            .ToListAsync(cancellationToken);
        return new(pagina, TamanhoPagina, total, itens.AsReadOnly());
    }

    private static RebanhoGravadoDto Gravado(Rebanho rebanho) => new(rebanho.Id, rebanho.Versao, rebanho.UltimoScoreId);

    private void Desanexar(Rebanho rebanho, IdentificacaoRebanho identificacaoAnterior, ScoreHistoricoRegistro? score = null)
    {
        contexto.Entry(rebanho.Identificacao).State = EntityState.Detached;
        contexto.Entry(identificacaoAnterior).State = EntityState.Detached;
        contexto.Entry(rebanho).State = EntityState.Detached;
        if (score is not null)
            contexto.Entry(score).State = EntityState.Detached;
    }
}
