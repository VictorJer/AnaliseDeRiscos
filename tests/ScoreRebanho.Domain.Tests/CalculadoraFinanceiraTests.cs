using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Producao;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class CalculadoraFinanceiraTests
{
    private readonly CalculadoraFinanceira calculadora = new(PremissasProducao.Versao1, PremissasFinanceiras.Versao1);

    [TestMethod]
    public void CenarioNumericoUsaArrobaVivaDeTrintaKgEMargemSobreCusto()
    {
        var resultado = calculadora.Calcular(250000m, 30000m);
        Assert.AreEqual(1000m, resultado.ArrobasVivasVendidas);
        Assert.AreEqual(340000m, resultado.Referencia.ReceitaReais);
        Assert.AreEqual(90000m, resultado.Referencia.LucroReais);
        Assert.AreEqual(0.36m, resultado.Referencia.MargemDecimal);
        Assert.AreEqual(250000m / 30000m, resultado.PrecoEquilibrioPorKgReais);
    }

    [TestMethod]
    public void QuedaDePrecoAlteraSomenteReceitaELucroEMargem()
    {
        var resultado = calculadora.Calcular(250000m, 30000m);
        Assert.AreEqual(306m, resultado.Adverso.PrecoArrobaVivaReais);
        Assert.AreEqual(306000m, resultado.Adverso.ReceitaReais);
        Assert.AreEqual(56000m, resultado.Adverso.LucroReais);
        Assert.AreEqual(0.224m, resultado.Adverso.MargemDecimal);
        Assert.AreEqual(30000m, resultado.PesoTotalVendidoKg);
        Assert.AreEqual(250000m, resultado.CustoTotalReais);
    }

    [TestMethod]
    public void ResultadoNegativoNaoEConvertidoEmZero()
    {
        var resultado = calculadora.Calcular(400000m, 30000m);
        Assert.AreEqual(-60000m, resultado.Referencia.LucroReais);
        Assert.AreEqual(-0.15m, resultado.Referencia.MargemDecimal);
        Assert.AreEqual(-94000m, resultado.Adverso.LucroReais);
    }

    [TestMethod]
    public void CustoZeroRetornaMargemIndisponivelComExplicacao()
    {
        var resultado = calculadora.Calcular(0m, 30000m);
        Assert.IsNull(resultado.Referencia.MargemDecimal);
        Assert.IsNull(resultado.Adverso.MargemDecimal);
        Assert.IsNotNull(resultado.MotivoMargemIndisponivel);
        Assert.AreEqual(0m, resultado.PrecoEquilibrioPorKgReais);
    }

    [TestMethod]
    public void PesoZeroRetornaEquilibrioIndisponivelComExplicacao()
    {
        var resultado = calculadora.Calcular(100m, 0m);
        Assert.IsNull(resultado.PrecoEquilibrioPorKgReais);
        Assert.IsNotNull(resultado.MotivoEquilibrioIndisponivel);
        Assert.AreEqual(-100m, resultado.Referencia.LucroReais);
        Assert.AreEqual(-1m, resultado.Referencia.MargemDecimal);
    }

    [TestMethod]
    [DataRow(-1, 1)]
    [DataRow(1, -1)]
    public void ValoresNegativosSaoRecusados(int custo, int peso)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => calculadora.Calcular(custo, peso));
    }

    [TestMethod]
    public void RejeitaPremissasInvalidas()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CalculadoraFinanceira(PremissasProducao.Versao1, PremissasFinanceiras.Versao1 with { PrecoVendaArrobaVivaReais = 0m }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CalculadoraFinanceira(PremissasProducao.Versao1, PremissasFinanceiras.Versao1 with { QuedaPrecoCenarioAdverso = 1.01m }));
    }

    [TestMethod]
    public void PreservaPrecisaoEDeterminismo()
    {
        var resultado = calculadora.Calcular(100.123m, 31.456m);
        Assert.AreEqual(31.456m / 30m * 340m, resultado.Referencia.ReceitaReais);
        Assert.AreEqual(resultado, calculadora.Calcular(100.123m, 31.456m));
    }

    [TestMethod]
    public void EstouroNumericoNaoRetornaResultadoParcial()
    {
        Assert.Throws<OverflowException>(() => calculadora.Calcular(1m, decimal.MaxValue));
    }
}
