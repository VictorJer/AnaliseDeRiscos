using System.Text.Json;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Infrastructure.Persistencia.Historico;

internal sealed class ScoreHistoricoRegistro
{
    public Guid Id { get; private set; }
    public string AutorId { get; private set; } = "";
    public Guid? RebanhoOrigemId { get; private set; }
    public string? NomeRebanho { get; private set; }
    public string? Propriedade { get; private set; }
    public DateTimeOffset CalculadoEmUtc { get; private set; }
    public string VersaoPremissas { get; private set; } = "";
    public int VersaoFormato { get; private set; }
    public string AnaliseJson { get; private set; } = "";
    public int ScoreTotal { get; private set; }
    public int Classificacao { get; private set; }

    private ScoreHistoricoRegistro() { }

    internal static ScoreHistoricoRegistro Criar(Guid id, string autorId, AnaliseCompletaDto analise,
        Guid? rebanhoId = null, IdentificacaoRebanho? identificacao = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentNullException.ThrowIfNull(analise);
        return new()
        {
            Id = id, AutorId = autorId, RebanhoOrigemId = rebanhoId,
            NomeRebanho = identificacao?.Nome, Propriedade = identificacao?.Propriedade,
            CalculadoEmUtc = analise.CalculadaEmUtc.ToUniversalTime(),
            VersaoPremissas = analise.Premissas.Versao, VersaoFormato = 1,
            AnaliseJson = JsonSerializer.Serialize(analise),
            ScoreTotal = analise.Score.Total, Classificacao = (int)analise.Score.Classificacao
        };
    }

    internal AnaliseCompletaDto LerAnalise()
    {
        if (VersaoFormato != 1)
            throw new InvalidOperationException("Versão de snapshot histórico não suportada.");
        return JsonSerializer.Deserialize<AnaliseCompletaDto>(AnaliseJson)
            ?? throw new InvalidOperationException("Snapshot histórico não pôde ser lido.");
    }
}
