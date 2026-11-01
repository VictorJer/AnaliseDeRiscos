using ScoreRebanho.Domain.Pastagens;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class CalculadoraLotacaoTests
{
    private readonly CalculadoraLotacao calculadora = new(PremissasLotacao.Versao1);

    [TestMethod]
    [DataRow(25, 0.25)]
    [DataRow(50, 0.5)]
    [DataRow(100, 1.0)]
    [DataRow(150, 1.5)]
    [DataRow(200, 2.0)]
    public void CalculaOcupacaoSemArredondamento(int animais, double ocupacao)
    {
        var resultado = calculadora.Calcular(new(100m, TipoPastagem.Verao, animais, 450m, false, null));
        Assert.AreEqual((decimal)ocupacao, resultado.OcupacaoDecimal);
        Assert.AreEqual(100m, resultado.QuantidadeMaximaAnimais);
        Assert.AreEqual("1.0", resultado.VersaoPremissas);
    }

    [TestMethod]
    [DataRow(1, 1.0)]
    [DataRow(2, 1.0)]
    [DataRow(3, 1.3)]
    [DataRow(5, 1.6)]
    [DataRow(100, 1.6)]
    public void RotacaoRespeitaMinimoETeto(int piquetes, double multiplicador)
    {
        var resultado = calculadora.Calcular(new(10m, TipoPastagem.Verao, 10, 450m, true, piquetes));
        Assert.AreEqual((decimal)multiplicador, resultado.MultiplicadorRotacao);
        Assert.AreEqual(10m * (decimal)multiplicador, resultado.CapacidadeUa);
        Assert.AreEqual(10m, resultado.CapacidadeSemRotacaoUa);
    }

    [TestMethod]
    public void InvernoUsaCapacidadeEspecificaEArredondaAnimaisParaBaixo()
    {
        var resultado = calculadora.Calcular(new(1m, TipoPastagem.Inverno, 1, 400m, false, null));
        Assert.AreEqual(1.2m, resultado.CapacidadeUa);
        Assert.AreEqual(1m, resultado.QuantidadeMaximaAnimais);
    }

    [TestMethod]
    public void PiquetesNaoAlteramCapacidadeQuandoRotacaoEstaDesativada()
    {
        var resultado = calculadora.Calcular(new(10m, TipoPastagem.Verao, 10, 450m, false, 5));
        Assert.AreEqual(1m, resultado.MultiplicadorRotacao);
    }

    [TestMethod]
    public void MesmasEntradasProduzemMesmoResultado()
    {
        var dados = new DadosLotacao(30m, TipoPastagem.Inverno, 50, 320m, true, 3);
        Assert.AreEqual(calculadora.Calcular(dados), calculadora.Calcular(dados));
    }

    [TestMethod]
    [DataRow(0, 1, 450)]
    [DataRow(-1, 1, 450)]
    [DataRow(1, 0, 450)]
    [DataRow(1, -1, 450)]
    [DataRow(1, 1, 0)]
    [DataRow(1, 1, -1)]
    public void RejeitaAreaQuantidadeOuPesoInvalidos(int area, int animais, int peso)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DadosLotacao(area, TipoPastagem.Verao, animais, peso, false, null));
    }

    [TestMethod]
    public void RejeitaPastagemDesconhecida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DadosLotacao(1, (TipoPastagem)99, 1, 450, false, null));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void RotacaoExigePiquetesPositivos(int? piquetes)
    {
        Assert.Throws<ArgumentException>(() => new DadosLotacao(1, TipoPastagem.Verao, 1, 450, true, piquetes));
    }

    [TestMethod]
    public void RejeitaPremissasQuePermitiriamDivisaoPorZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PremissasLotacao("1.0", 0, 1, 1.2m, 0.15m, 1.6m, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PremissasLotacao("1.0", 450, 0, 1.2m, 0.15m, 1.6m, 3));
    }

    [TestMethod]
    public void EstouroNaoProduzResultadoParcial()
    {
        Assert.Throws<OverflowException>(() => calculadora.Calcular(new(1m, TipoPastagem.Verao, 2, decimal.MaxValue, false, null)));
    }
}
