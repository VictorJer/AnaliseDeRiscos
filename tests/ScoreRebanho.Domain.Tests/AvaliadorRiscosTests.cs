using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class AvaliadorRiscosTests
{
    private readonly AvaliadorRiscos avaliador = new(PremissasAnalise.Versao1);
    private static DadosSimulacao Dados => new(new(100, TipoPastagem.Verao, 100, 450, false, null),
        TipoPosse.Proprio, null, true, 10, 180, false, new(true, true, true, false));
    private static ResultadoLotacao Lotacao(decimal ocupacao) => new("1.0", 100, 1, 100, 100, ocupacao, 100, 100);
    private static ResultadoFinanceiro Financeiro(decimal margem, decimal lucroAdverso = 10m) =>
        new("1.0", 100, 10, 10m / 30m, 10, null, null,
            new(340, 100m * (1m + margem), 100m * margem, margem),
            new(306, 100m + lucroAdverso, lucroAdverso, lucroAdverso / 100m));

    [TestMethod]
    [DataRow(0.2, false)]
    [DataRow(0.19, true)]
    [DataRow(0.0, true)]
    public void MargemBaixaUsaLimiteDaPontuacaoFinanceira(double margem, bool esperado)
    {
        var alertas = avaliador.Avaliar(Dados, Lotacao(1m), Financeiro((decimal)margem));
        Assert.AreEqual(esperado, alertas.Any(a => a.Codigo == "MARGEM_BAIXA"));
    }

    [TestMethod]
    public void PrejuizoNaoEVendidoComoSomenteMargemBaixa()
    {
        var alertas = avaliador.Avaliar(Dados, Lotacao(1m), Financeiro(-0.1m, -15));
        Assert.IsTrue(alertas.Any(a => a.Codigo == "PREJUIZO"));
        Assert.IsFalse(alertas.Any(a => a.Codigo == "MARGEM_BAIXA"));
    }

    [TestMethod]
    [DataRow(0.49, "SUBOCUPACAO")]
    [DataRow(1.01, "SUPERLOTACAO")]
    public void OcupacaoForaDaFaixaGeraRecomendacao(double ocupacao, string codigo)
    {
        var alertas = avaliador.Avaliar(Dados, Lotacao((decimal)ocupacao), Financeiro(0.2m));
        Assert.IsTrue(alertas.Any(a => a.Codigo == codigo && !string.IsNullOrWhiteSpace(a.Recomendacao)));
    }

    [TestMethod]
    [DataRow(0.5)]
    [DataRow(1.0)]
    public void LimitesIdeaisNaoGeramAlertaDeLotacao(double ocupacao)
    {
        Assert.IsEmpty(avaliador.Avaliar(Dados, Lotacao((decimal)ocupacao), Financeiro(0.2m)));
    }

    [TestMethod]
    public void CenarioAdversoGeraAlertaQuandoLucroPassaAbaixoDeZero()
    {
        var alertas = avaliador.Avaliar(Dados, Lotacao(1m), Financeiro(0.05m, -5));
        Assert.IsTrue(alertas.Any(a => a.Codigo == "PREJUIZO_CENARIO_ADVERSO"));
        Assert.IsFalse(avaliador.Avaliar(Dados, Lotacao(1m), Financeiro(0.05m, 0))
            .Any(a => a.Codigo == "PREJUIZO_CENARIO_ADVERSO"));
    }

    [TestMethod]
    public void ResultadoEListaSomenteLeituraComOrdemDeterministica()
    {
        var primeiro = avaliador.Avaliar(Dados, Lotacao(2m), Financeiro(-0.1m));
        var segundo = avaliador.Avaliar(Dados, Lotacao(2m), Financeiro(-0.1m));
        Assert.AreEqual("PREJUIZO", primeiro[0].Codigo);
        Assert.AreEqual("SUPERLOTACAO", primeiro[1].Codigo);
        CollectionAssert.AreEqual(primeiro.ToArray(), segundo.ToArray());
        Assert.Throws<NotSupportedException>(() => ((IList<AlertaAnalise>)primeiro).Clear());
    }
}
