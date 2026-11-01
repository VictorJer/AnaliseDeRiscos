using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class CalculadoraCustosTests
{
    private readonly CalculadoraCustos calculadora = new(PremissasLotacao.Versao1, PremissasCustos.Versao1);

    private static DadosSimulacao Dados(int animais = 100, int dias = 365, bool pronto = true,
        TipoPastagem pastagem = TipoPastagem.Verao, int? piquetes = null, ManejoSanitario? manejo = null,
        decimal? aluguel = null) =>
        new(new(100m, pastagem, animais, 450m, piquetes.HasValue, piquetes),
            aluguel.HasValue ? TipoPosse.Arrendado : TipoPosse.Proprio, aluguel, pronto, 10m, dias,
            false, manejo ?? new(false, false, false, false));

    [TestMethod]
    public void CompraEManutencaoCompoemCustoMasSoCompraEntraNoCapital()
    {
        var resultado = calculadora.Calcular(Dados());
        Assert.AreEqual(480000m, resultado.CustoTotalReais);
        Assert.AreEqual(450000m, resultado.CapitalInicialReais);
        Assert.AreEqual(resultado.Itens.Sum(i => i.CustoOperacaoReais), resultado.CustoTotalReais);
        Assert.AreEqual(resultado.Itens.Sum(i => i.CapitalInicialReais), resultado.CapitalInicialReais);
        Assert.IsEmpty(resultado.Produtos);
    }

    [TestMethod]
    public void FormacaoVeraoUsaUmQuintoNoCustoEInvestimentoIntegralNoCapital()
    {
        var resultado = calculadora.Calcular(Dados(pronto: false));
        Assert.AreEqual(527700m, resultado.CustoTotalReais);
        Assert.AreEqual(688500m, resultado.CapitalInicialReais);
        Assert.AreEqual(5, resultado.Itens.Count(i => i.Descricao.StartsWith("Formação")));
    }

    [TestMethod]
    public void FormacaoInvernoEIntegral()
    {
        var resultado = calculadora.Calcular(Dados(pronto: false, pastagem: TipoPastagem.Inverno));
        Assert.AreEqual(735000m, resultado.CustoTotalReais);
        Assert.AreEqual(690000m, resultado.CapitalInicialReais);
    }

    [TestMethod]
    public void RotacaoComDoisPiquetesTemCustoSemBeneficioDeCapacidade()
    {
        var resultado = calculadora.Calcular(Dados(piquetes: 2));
        var item = resultado.Itens.Single(i => i.Descricao == "Rotação do pasto");
        Assert.AreEqual(2700m, item.CustoOperacaoReais);
        Assert.AreEqual(27000m, item.CapitalInicialReais);
    }

    [TestMethod]
    [DataRow(100, 0)]
    [DataRow(110, 0)]
    [DataRow(120, 3600)]
    [DataRow(210, 63000)]
    public void SuplementacaoCresceApenasAcimaDeCentoEDezPorCento(int animais, int custo)
    {
        var resultado = calculadora.Calcular(Dados(animais, dias: 100));
        var item = resultado.Itens.Single(i => i.Descricao == "Suplementação");
        Assert.AreEqual((decimal)custo, item.CustoOperacaoReais);
        Assert.AreEqual(0m, item.CapitalInicialReais);
    }

    [TestMethod]
    public void ArrendamentoEProporcionalAosDiasEFicaForaDoCapital()
    {
        var resultado = calculadora.Calcular(Dados(dias: 45, aluguel: 1000m));
        var item = resultado.Itens.Single(i => i.Descricao == "Arrendamento");
        Assert.AreEqual(1500m, item.CustoOperacaoReais);
        Assert.AreEqual(0m, item.CapitalInicialReais);
    }

    [TestMethod]
    [DataRow(89, 1)]
    [DataRow(90, 2)]
    [DataRow(179, 2)]
    [DataRow(180, 3)]
    public void CarrapatoIncluiDiaInicialEUltimoDiaCoincidente(int dias, int aplicacoes)
    {
        var resultado = calculadora.Calcular(Dados(101, dias, manejo: new(false, true, false, false)));
        var produto = resultado.Produtos.Single();
        Assert.AreEqual((decimal)aplicacoes, produto.Aplicacoes);
        Assert.AreEqual(404m * aplicacoes, produto.QuantidadeNecessaria);
        Assert.AreEqual(decimal.Ceiling(404m * aplicacoes / 500m), produto.QuantidadeEmbalagens);
    }

    [TestMethod]
    [DataRow(149, 1)]
    [DataRow(150, 2)]
    [DataRow(300, 3)]
    public void VermifugoRespeitaIntervaloDeCentoECinquentaDias(int dias, int aplicacoes)
    {
        var produto = calculadora.Calcular(Dados(100, dias, manejo: new(false, false, true, false))).Produtos.Single();
        Assert.AreEqual((decimal)aplicacoes, produto.Aplicacoes);
        Assert.AreEqual(600m * aplicacoes, produto.QuantidadeNecessaria);
    }

    [TestMethod]
    public void FrascosESacosArredondamParaCimaEEntramNoCapital()
    {
        var resultado = calculadora.Calcular(Dados(101, 1, manejo: new(true, false, false, true)));
        var vacina = resultado.Produtos.Single(p => p.Produto == "Vacina de carbúnculo");
        var modificador = resultado.Produtos.Single(p => p.Produto == "Modificador orgânico");
        Assert.AreEqual(3m, vacina.QuantidadeEmbalagens);
        Assert.AreEqual(120m, vacina.CustoReais);
        Assert.AreEqual(5.05m, modificador.QuantidadeNecessaria);
        Assert.AreEqual(1m, modificador.QuantidadeEmbalagens);
        Assert.AreEqual(454740m, resultado.CapitalInicialReais);
    }

    [TestMethod]
    public void FrascosConsideramVolumeTotalSemArredondarPorAplicacao()
    {
        var produto = calculadora.Calcular(Dados(1, 90, manejo: new(false, true, false, false))).Produtos.Single();
        Assert.AreEqual(8m, produto.QuantidadeNecessaria);
        Assert.AreEqual(1m, produto.QuantidadeEmbalagens);
    }

    [TestMethod]
    public void DiasMuitoGrandesNaoEstouramContagemDeAplicacoes()
    {
        var premissas = PremissasCustos.Versao1 with { Carrapato = new(4, 500, 100, 1) };
        var resultado = new CalculadoraCustos(PremissasLotacao.Versao1, premissas)
            .Calcular(Dados(1, int.MaxValue, manejo: new(false, true, false, false)));
        Assert.AreEqual(2147483648m, resultado.Produtos.Single().Aplicacoes);
    }

    [TestMethod]
    public void RejeitaPremissasInvalidasAoConstruirCalculadora()
    {
        foreach (var p in new[]
        {
            PremissasCustos.Versao1 with { DiasAno = 0 },
            PremissasCustos.Versao1 with { ModificadorKgPorSaco = 0 },
            PremissasCustos.Versao1 with { FracaoEconomicaFormacaoVerao = 2m },
            PremissasCustos.Versao1 with { Carrapato = new(4, 0, 100, 90) },
            PremissasCustos.Versao1 with { Vermifugo = new(6, 500, 85, 0) },
            PremissasCustos.Versao1 with { ManutencaoVeraoReaisHaAno = -1 }
        })
            Assert.Throws<ArgumentOutOfRangeException>(() => new CalculadoraCustos(PremissasLotacao.Versao1, p));
        Assert.Throws<ArgumentException>(() => new CalculadoraCustos(PremissasLotacao.Versao1, PremissasCustos.Versao1 with { Versao = "2.0" }));
    }

    [TestMethod]
    public void EstouroNaoRetornaCustoParcial()
    {
        var dados = new DadosSimulacao(new(1, TipoPastagem.Verao, 1, 450, false, null),
            TipoPosse.Proprio, null, true, decimal.MaxValue, 1, false, new(false, false, false, false));
        Assert.Throws<OverflowException>(() => calculadora.Calcular(dados));
    }

    [TestMethod]
    public void CustoPreservaPrecisaoEDeterminismo()
    {
        var dados = Dados(dias: 1);
        var primeiro = calculadora.Calcular(dados);
        var segundo = calculadora.Calcular(dados);
        Assert.AreEqual(30000m / 365m, primeiro.Itens.Single(i => i.Descricao == "Manutenção do pasto").CustoOperacaoReais);
        Assert.AreEqual(primeiro.CustoTotalReais, segundo.CustoTotalReais);
        CollectionAssert.AreEqual(primeiro.Produtos.ToArray(), segundo.Produtos.ToArray());
    }
}
