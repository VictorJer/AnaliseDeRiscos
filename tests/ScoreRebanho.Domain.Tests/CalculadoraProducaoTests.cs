using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Producao;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class CalculadoraProducaoTests
{
    private readonly CalculadoraProducao calculadora = new(PremissasLotacao.Versao1, PremissasProducao.Versao1);

    private static DadosSimulacao Dados(int animais, ManejoSanitario? manejo = null, bool raca = false) =>
        new(new(100m, TipoPastagem.Verao, animais, 450m, false, null),
            TipoPosse.Proprio, null, true, 10m, 100, raca, manejo ?? new(true, true, true, false));

    [TestMethod]
    public void CenarioNormalMantemPrecisaoEAnimaisFracionarios()
    {
        var resultado = calculadora.Calcular(Dados(100));
        Assert.AreEqual(0.64m, resultado.GmdKgDia);
        Assert.AreEqual(514m, resultado.PesoVendaPorAnimalKg);
        Assert.AreEqual(0.015m, resultado.MortalidadeDecimal);
        Assert.AreEqual(98.5m, resultado.AnimaisVendidosEstimados);
        Assert.AreEqual(50629m, resultado.PesoTotalVendidoKg);
        Assert.AreEqual(50629m / 30m, resultado.ArrobasVivasVendidas);
    }

    [TestMethod]
    public void BonusDeRacaEModificadorSaoAplicadosAntesDaSuperlotacao()
    {
        var resultado = calculadora.Calcular(Dados(200, new(true, true, true, true), true));
        Assert.AreEqual(0.5m, resultado.FatorGmd);
        Assert.AreEqual(0.43m, resultado.GmdKgDia);
        Assert.AreEqual(0.055m, resultado.MortalidadeDecimal);
    }

    [TestMethod]
    public void AusenciasSanitariasAumentamMortalidade()
    {
        var resultado = calculadora.Calcular(Dados(100, new(false, false, false, false)));
        Assert.AreEqual(0.05m, resultado.MortalidadeDecimal);
        Assert.AreEqual(0.55m, resultado.GmdKgDia);
    }

    [TestMethod]
    public void SuperlotacaoExtremaRespeitaTetoDeMortalidadeEPisoDeGmd()
    {
        var resultado = calculadora.Calcular(Dados(1000, new(false, false, false, false)));
        Assert.AreEqual(0.15m, resultado.MortalidadeDecimal);
        Assert.AreEqual(0.45m, resultado.FatorGmd);
        Assert.AreEqual(850m, resultado.AnimaisVendidosEstimados);
    }

    [TestMethod]
    [DataRow(25)]
    [DataRow(50)]
    [DataRow(100)]
    public void OcupacaoAteCapacidadeNaoPenalizaProducao(int animais)
    {
        var dados = Dados(animais);
        var primeiro = calculadora.Calcular(dados);
        Assert.AreEqual(1m, primeiro.FatorGmd);
        Assert.AreEqual(0.015m, primeiro.MortalidadeDecimal);
        Assert.AreEqual(primeiro, calculadora.Calcular(dados));
        Assert.AreEqual(animais, dados.Lotacao.QuantidadeAnimais);
    }

    [TestMethod]
    public void NaoAceitaPremissasAusentes()
    {
        Assert.Throws<ArgumentNullException>(() => new CalculadoraProducao(PremissasLotacao.Versao1, null!));
    }
}
