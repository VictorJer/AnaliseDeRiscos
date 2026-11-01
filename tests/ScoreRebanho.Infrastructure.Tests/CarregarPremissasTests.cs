using Microsoft.Extensions.Configuration;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;
using ScoreRebanho.Infrastructure.Configuracao;

namespace ScoreRebanho.Infrastructure.Tests;

[TestClass]
public sealed class CarregarPremissasTests
{
    private static IConfiguration Configuracao(params (string Chave, string? Valor)[] alteracoes)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "premissas-padrao.json"))
            .AddInMemoryCollection(alteracoes.ToDictionary(a => a.Chave, a => a.Valor)).Build();
    }

    [TestMethod]
    public void CarregaTodosOsValoresAprovadosSemPerdaDePrecisao()
    {
        var premissas = CarregarPremissas.DaConfiguracao(Configuracao());
        Assert.AreEqual(PremissasAnalise.Versao1, premissas);
    }

    [TestMethod]
    public void SubstituicaoDeConfiguracaoAfetaCalculoESnapshotSemModificarResultadoAnterior()
    {
        var entrada = new EntradaSimulacaoDto
        {
            AreaHectares = 100, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
            PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450,
            PrecoCompraPorKgReais = 10, DiasPastagem = 180
        };
        var original = new CalcularScoreRebanho(CarregarPremissas.DaConfiguracao(Configuracao()), TimeProvider.System)
            .Executar(entrada).Value;
        var config = Configuracao(
            ("Premissas:Lotacao:Versao", "1.1"), ("Premissas:Producao:Versao", "1.1"),
            ("Premissas:Custos:Versao", "1.1"), ("Premissas:Financeiro:Versao", "1.1"),
            ("Premissas:Score:Versao", "1.1"), ("Premissas:Financeiro:PrecoVendaArrobaVivaReais", "350.125"));
        var atualizado = new CalcularScoreRebanho(CarregarPremissas.DaConfiguracao(config), TimeProvider.System)
            .Executar(entrada).Value;
        Assert.AreEqual(350.125m, atualizado.Financeiro.Referencia.PrecoArrobaVivaReais);
        Assert.AreEqual("1.1", atualizado.Premissas.Versao);
        Assert.AreEqual(350.125m, atualizado.Premissas.Financeiro.PrecoVendaArrobaVivaReais);
        Assert.AreEqual(340m, original.Financeiro.Referencia.PrecoArrobaVivaReais);
        Assert.AreEqual("1.0", original.Premissas.Versao);
        Assert.AreEqual(original.Producao, atualizado.Producao);
        Assert.AreEqual(original.Custos.CustoTotalReais, atualizado.Custos.CustoTotalReais);
        Assert.IsGreaterThan(original.Financeiro.Referencia.ReceitaReais, atualizado.Financeiro.Referencia.ReceitaReais);
    }

    [TestMethod]
    [DataRow("Premissas:Lotacao:QuilogramasPorUa", "0")]
    [DataRow("Premissas:Producao:MortalidadeMaxima", "1")]
    [DataRow("Premissas:Custos:Carrapato:VolumeFrascoMl", "0")]
    [DataRow("Premissas:Custos:FormacaoVerao:SementeReaisPorHectare", "-1")]
    [DataRow("Premissas:Financeiro:QuedaPrecoCenarioAdverso", "1.01")]
    [DataRow("Premissas:Score:OcupacaoSemPontos", "0.5")]
    [DataRow("Premissas:Score:Versao", "outra")]
    [DataRow("Premissas:Financeiro:PrecoVendaArrobaVivaReais", "texto")]
    [DataRow("Premissas:Financeiro:PrecoVendaArrobaVivaReais", "999999999999999999999999999999999")]
    [DataRow("Premissas:Financeiro:PrecoInexistente", "350")]
    public void RecusaConfiguracaoInvalidaAntesDeCalcular(string chave, string valor)
    {
        var erro = Assert.Throws<InvalidOperationException>(() =>
            CarregarPremissas.DaConfiguracao(Configuracao((chave, valor))));
        StringAssert.Contains(erro.Message, "configuração de Premissas é inválida");
    }

    [TestMethod]
    public void SecaoAusenteImpedeInicializacao()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CarregarPremissas.DaConfiguracao(new ConfigurationBuilder().Build()));
    }
}
