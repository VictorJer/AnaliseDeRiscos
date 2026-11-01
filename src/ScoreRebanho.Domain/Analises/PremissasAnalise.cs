using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Producao;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Domain.Analises;

public sealed record PremissasAnalise
{
    public PremissasLotacao Lotacao { get; }
    public PremissasProducao Producao { get; }
    public PremissasCustos Custos { get; }
    public PremissasFinanceiras Financeiro { get; }
    public PremissasScore Score { get; }
    public string Versao => Lotacao.Versao;
    public static PremissasAnalise Versao1 { get; } = new(
        PremissasLotacao.Versao1, PremissasProducao.Versao1, PremissasCustos.Versao1,
        PremissasFinanceiras.Versao1, PremissasScore.Versao1);

    public PremissasAnalise(PremissasLotacao lotacao, PremissasProducao producao,
        PremissasCustos custos, PremissasFinanceiras financeiro, PremissasScore score)
    {
        ArgumentNullException.ThrowIfNull(lotacao);
        ArgumentNullException.ThrowIfNull(producao);
        ArgumentNullException.ThrowIfNull(custos);
        ArgumentNullException.ThrowIfNull(financeiro);
        ArgumentNullException.ThrowIfNull(score);
        custos.Validar();
        financeiro.Validar();
        score.Validar();
        if (new[] { producao.Versao, custos.Versao, financeiro.Versao, score.Versao }.Any(v => v != lotacao.Versao))
            throw new ArgumentException("Todas as premissas da análise devem ter a mesma versão.");
        Lotacao = lotacao;
        Producao = producao;
        Custos = custos;
        Financeiro = financeiro;
        Score = score;
    }
}
