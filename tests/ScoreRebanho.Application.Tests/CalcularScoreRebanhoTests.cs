using System.Text.Json;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Scores;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public sealed class CalcularScoreRebanhoTests
{
    private static readonly DateTimeOffset Data = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly CalcularScoreRebanho caso = new(new RelogioFixo());

    private static EntradaSimulacaoDto Entrada => new()
    {
        AreaHectares = 100m, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
        PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450m,
        PrecoCompraPorKgReais = 10m, DiasPastagem = 365,
        VacinaCarbunculo = true, ControleCarrapato = true, Vermifugo = true
    };

    [TestMethod]
    public void IntegraTodosOsCalculosComTotaisCoerentes()
    {
        var resultado = caso.Executar(Entrada);
        Assert.IsTrue(resultado.IsSuccess);
        var analise = resultado.Value;
        Assert.AreEqual(Data, analise.CalculadaEmUtc);
        Assert.AreEqual(1m, analise.Lotacao.OcupacaoDecimal);
        Assert.AreEqual(0.64m, analise.Producao.GmdKgDia);
        Assert.AreEqual(683.6m, analise.Producao.PesoVendaPorAnimalKg);
        Assert.AreEqual(67334.6m, analise.Producao.PesoTotalVendidoKg);
        Assert.AreEqual(480820m, analise.Custos.CustoTotalReais);
        Assert.AreEqual(450820m, analise.Custos.CapitalInicialReais);
        Assert.AreEqual(analise.Custos.Itens.Sum(i => i.CustoOperacaoReais), analise.Custos.CustoTotalReais);
        var receita = 67334.6m / 30m * 340m;
        Assert.AreEqual(receita, analise.Financeiro.Referencia.ReceitaReais);
        Assert.AreEqual(receita - 480820m, analise.Financeiro.Referencia.LucroReais);
        Assert.AreEqual(87, analise.Score.Total);
        Assert.AreEqual(ClassificacaoRisco.Baixo, analise.Score.Classificacao);
        Assert.HasCount(4, analise.Score.Componentes);
        Assert.IsEmpty(analise.Alertas);
        Assert.IsFalse(string.IsNullOrWhiteSpace(analise.ExplicacaoResumida));
    }

    [TestMethod]
    public void PreservaEntradasEPremissasComValoresNumericosParaHistorico()
    {
        var entrada = Entrada;
        var analise = caso.Executar(entrada).Value;
        Assert.IsNull(entrada.ArrendamentoMensalReais);
        Assert.AreEqual(0m, analise.Entradas.ArrendamentoMensalReais);
        Assert.AreEqual("1.0", analise.Premissas.Versao);
        Assert.AreEqual(340m, analise.Premissas.Financeiro.PrecoVendaArrobaVivaReais);
        Assert.AreEqual(30m, analise.Premissas.Producao.QuilogramasPorArrobaViva);
        Assert.AreEqual(450m, analise.Premissas.Lotacao.QuilogramasPorUa);
        Assert.AreEqual(90, analise.Premissas.Custos.Carrapato.IntervaloDias);
        Assert.AreEqual(80, analise.Premissas.LimiteRiscoBaixo);
        Assert.AreEqual(100m, analise.Premissas.ScoreMaximo);
        var json = JsonSerializer.Serialize(analise);
        var recuperada = JsonSerializer.Deserialize<AnaliseCompletaDto>(json);
        Assert.IsNotNull(recuperada);
        Assert.AreEqual(analise.Financeiro.Referencia, recuperada.Financeiro.Referencia);
        Assert.AreEqual(analise.Premissas, recuperada.Premissas);
        Assert.AreEqual(analise.Entradas, recuperada.Entradas);
    }

    [TestMethod]
    public void MesmasEntradasEPremissasMantemMesmoResultado()
    {
        Assert.AreEqual(JsonSerializer.Serialize(caso.Executar(Entrada).Value),
            JsonSerializer.Serialize(caso.Executar(Entrada).Value));
    }

    [TestMethod]
    public void PremissasPersonalizadasSaoUsadasEPreservadas()
    {
        var conjunto = PremissasAnalise.Versao1;
        var financeiro = conjunto.Financeiro with { PrecoVendaArrobaVivaReais = 300m };
        var personalizado = new CalcularScoreRebanho(
            new(conjunto.Lotacao, conjunto.Producao, conjunto.Custos, financeiro, conjunto.Score), new RelogioFixo());
        var resultado = personalizado.Executar(Entrada).Value;
        Assert.AreEqual(300m, resultado.Premissas.Financeiro.PrecoVendaArrobaVivaReais);
        Assert.AreEqual(270m, resultado.Financeiro.Adverso.PrecoArrobaVivaReais);
        Assert.AreEqual(caso.Executar(Entrada).Value.Custos.CustoTotalReais, resultado.Custos.CustoTotalReais);
    }

    [TestMethod]
    public void EntradasInvalidasRetornamFalhaComCodigoECampoSemPayload()
    {
        foreach (var (entrada, campo) in new (EntradaSimulacaoDto, string)[]
        {
            (Entrada with { AreaHectares = 0m }, "AreaHectares"),
            (Entrada with { Pastagem = (TipoPastagem)99 }, "Pastagem"),
            (Entrada with { Posse = (TipoPosse)99 }, "Posse"),
            (Entrada with { QuantidadeAnimais = -1 }, "QuantidadeAnimais"),
            (Entrada with { PesoCompraKg = 0 }, "PesoCompraKg"),
            (Entrada with { RotacaoAtiva = true, NumeroPiquetes = null }, "NumeroPiquetes"),
            (Entrada with { RotacaoAtiva = true, NumeroPiquetes = 0 }, "NumeroPiquetes"),
            (Entrada with { NumeroPiquetes = -1 }, "NumeroPiquetes"),
            (Entrada with { Posse = TipoPosse.Arrendado, ArrendamentoMensalReais = null }, "ArrendamentoMensalReais"),
            (Entrada with { ArrendamentoMensalReais = -1m }, "ArrendamentoMensalReais"),
            (Entrada with { PrecoCompraPorKgReais = 0 }, "PrecoCompraPorKgReais"),
            (Entrada with { DiasPastagem = 0 }, "DiasPastagem")
        })
        {
            var resultado = caso.Executar(entrada);
            Assert.IsTrue(resultado.IsFailed, campo);
            Assert.AreEqual("ENTRADA_INVALIDA", resultado.Errors[0].Metadata["Codigo"]);
            Assert.AreEqual(campo, resultado.Errors[0].Metadata["Campo"]);
            Assert.IsNull(resultado.ValueOrDefault);
            Assert.DoesNotContain("Exception", resultado.Errors[0].Message);
        }
    }

    [TestMethod]
    public void EstouroNumericoRetornaFalhaSemRelatorioParcial()
    {
        var resultado = caso.Executar(Entrada with { PrecoCompraPorKgReais = decimal.MaxValue });
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("LIMITE_NUMERICO", resultado.Errors[0].Metadata["Codigo"]);
        Assert.IsNull(resultado.ValueOrDefault);
    }

    [TestMethod]
    public void CapacidadeArredondadaAZeroRetornaFalhaNumerica()
    {
        var conjunto = PremissasAnalise.Versao1;
        var lotacao = new PremissasLotacao("1.0", 450m, 0.01m, 1.2m, 0.15m, 1.6m, 3);
        var personalizado = new CalcularScoreRebanho(new(lotacao, conjunto.Producao, conjunto.Custos,
            conjunto.Financeiro, conjunto.Score), new RelogioFixo());
        var resultado = personalizado.Executar(Entrada with { AreaHectares = 0.0000000000000000000000000001m });
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("LIMITE_NUMERICO", resultado.Errors[0].Metadata["Codigo"]);
        Assert.IsNull(resultado.ValueOrDefault);
    }

    [TestMethod]
    public void ConfiguracaoInvalidaFalhaNaConstrucaoEmVezDeVirarErroDeEntrada()
    {
        var conjunto = PremissasAnalise.Versao1;
        Assert.Throws<ArgumentOutOfRangeException>(() => new PremissasAnalise(conjunto.Lotacao,
            conjunto.Producao, conjunto.Custos with { DiasAno = 0 }, conjunto.Financeiro, conjunto.Score));
        Assert.Throws<ArgumentException>(() => new PremissasAnalise(conjunto.Lotacao,
            conjunto.Producao, conjunto.Custos, conjunto.Financeiro with { Versao = "2.0" }, conjunto.Score));
    }

    [TestMethod]
    public void EntradaNulaEDefeitosInesperadosNaoSaoOcultados()
    {
        Assert.Throws<ArgumentNullException>(() => caso.Executar(null!));
        Assert.Throws<InvalidOperationException>(() => new CalcularScoreRebanho(new RelogioComDefeito()).Executar(Entrada));
    }

    [TestMethod]
    public void ColecoesDeSaidaSaoSomenteLeitura()
    {
        var analise = caso.Executar(Entrada).Value;
        Assert.Throws<NotSupportedException>(() => ((IList<ItemCustoDto>)analise.Custos.Itens).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<ComponenteScoreDto>)analise.Score.Componentes).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)analise.Avisos).Clear());
    }

    [TestMethod]
    public void PrejuizoESuperlotacaoERiscosSanitariosPermanecemExplicitos()
    {
        var analise = caso.Executar(Entrada with
        {
            QuantidadeAnimais = 300, PrecoCompraPorKgReais = 30m,
            VacinaCarbunculo = false, ControleCarrapato = false, Vermifugo = false
        }).Value;
        Assert.AreEqual("PREJUIZO", analise.Alertas[0].Codigo);
        Assert.AreEqual("SUPERLOTACAO", analise.Alertas[1].Codigo);
        foreach (var codigo in new[] { "SEM_VACINA_CARBUNCULO", "SEM_CONTROLE_CARRAPATO", "SEM_VERMIFUGO" })
            Assert.IsTrue(analise.Alertas.Any(a => a.Codigo == codigo));
        Assert.IsTrue(analise.Alertas.All(a => !string.IsNullOrWhiteSpace(a.Recomendacao)));
        Assert.IsTrue(analise.Avisos.Any(a => a.Contains("não garante lucro")));
        Assert.IsTrue(analise.Avisos.Any(a => a.Contains("capital inicial não inclui")));
    }

    [TestMethod]
    public void AlertaDeRotacaoInsuficienteNaoEliminaSeuCusto()
    {
        var analise = caso.Executar(Entrada with { RotacaoAtiva = true, NumeroPiquetes = 2 }).Value;
        Assert.IsTrue(analise.Alertas.Any(a => a.Codigo == "ROTACAO_SEM_BENEFICIO"));
        Assert.IsGreaterThan(0m, analise.Custos.Itens.Single(i => i.Descricao == "Rotação do pasto").CapitalInicialReais);
    }

    [TestMethod]
    public void MargemIndisponivelNaoGeraScoreParcial()
    {
        var conjunto = PremissasAnalise.Versao1;
        var custos = conjunto.Custos with { ManutencaoVeraoReaisHaAno = 0m };
        var personalizado = new CalcularScoreRebanho(
            new(conjunto.Lotacao, conjunto.Producao, custos, conjunto.Financeiro, conjunto.Score), new RelogioFixo());
        var minimo = 0.0000000000000000000000000001m;
        var resultado = personalizado.Executar(Entrada with
        {
            AreaHectares = minimo, PesoCompraKg = minimo, PrecoCompraPorKgReais = minimo,
            QuantidadeAnimais = 1, DiasPastagem = 1,
            VacinaCarbunculo = false, ControleCarrapato = false, Vermifugo = false
        });
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("MARGEM_INDISPONIVEL", resultado.Errors[0].Metadata["Codigo"]);
        Assert.IsNull(resultado.ValueOrDefault);
    }

    private sealed class RelogioFixo : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Data;
    }

    private sealed class RelogioComDefeito : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => throw new InvalidOperationException("Defeito inesperado no relógio de teste.");
    }
}
