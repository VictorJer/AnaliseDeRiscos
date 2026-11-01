using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Scores;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class CalculadoraScoreTests
{
    private readonly CalculadoraScore calculadora = new(PremissasLotacao.Versao1, PremissasScore.Versao1);
    private static DadosSimulacao Dados(int dias = 180, bool raca = true, int piquetes = 3) =>
        new(new(100, TipoPastagem.Verao, 100, 450, true, piquetes), TipoPosse.Proprio,
            null, true, 10m, dias, raca, new(true, true, true, true));

    [TestMethod]
    [DataRow(0.0, 18.0)]
    [DataRow(0.25, 24.0)]
    [DataRow(0.5, 30.0)]
    [DataRow(1.0, 30.0)]
    [DataRow(1.4, 15.0)]
    [DataRow(1.8, 0.0)]
    [DataRow(2.0, 0.0)]
    public void LotacaoRespeitaTodasAsFaixas(double ocupacao, double esperado)
    {
        var resultado = calculadora.Calcular(Dados(), (decimal)ocupacao, 0.2m);
        Assert.AreEqual((decimal)esperado, resultado.Componentes.Single(c => c.Nome == "Lotação").Pontos);
    }

    [TestMethod]
    [DataRow(-0.2, 0.0)]
    [DataRow(-0.1, 0.0)]
    [DataRow(0.0, 10.0)]
    [DataRow(0.05, 15.0)]
    [DataRow(0.2, 30.0)]
    [DataRow(1.0, 30.0)]
    public void FinanceiroInterpolaELimitaPontuacao(double margem, double esperado)
    {
        Assert.AreEqual((decimal)esperado, calculadora.Calcular(Dados(), 1m, (decimal)margem)
            .Componentes.Single(c => c.Nome == "Resultado financeiro").Pontos);
    }

    [TestMethod]
    public void CenarioCompletoAlcancaCemPontos()
    {
        var resultado = calculadora.Calcular(Dados(), 1m, 0.2m);
        Assert.AreEqual(100, resultado.Total);
        Assert.AreEqual(25m, resultado.Componentes.Single(c => c.Nome == "Sanidade").Pontos);
        Assert.AreEqual(15m, resultado.Componentes.Single(c => c.Nome == "Manejo e custos fixos").Pontos);
        Assert.AreEqual(ClassificacaoRisco.Baixo, resultado.Classificacao);
    }

    [TestMethod]
    [DataRow(119, 13)]
    [DataRow(120, 15)]
    [DataRow(365, 15)]
    [DataRow(366, 13)]
    public void PrazoIdealIncluiOsLimites(int dias, int pontos)
    {
        Assert.AreEqual((decimal)pontos, calculadora.Calcular(Dados(dias), 1m, 0.2m)
            .Componentes.Single(c => c.Nome == "Manejo e custos fixos").Pontos);
    }

    [TestMethod]
    public void DoisPiquetesNaoConcedemPontosDeRotacao()
    {
        Assert.AreEqual(11m, calculadora.Calcular(Dados(piquetes: 2), 1m, 0.2m)
            .Componentes.Single(c => c.Nome == "Manejo e custos fixos").Pontos);
    }

    [TestMethod]
    [DataRow(39.49, 39, ClassificacaoRisco.MuitoAlto)]
    [DataRow(39.5, 40, ClassificacaoRisco.Alto)]
    [DataRow(59.49, 59, ClassificacaoRisco.Alto)]
    [DataRow(59.5, 60, ClassificacaoRisco.Moderado)]
    [DataRow(79.49, 79, ClassificacaoRisco.Moderado)]
    [DataRow(79.5, 80, ClassificacaoRisco.Baixo)]
    public void ArredondaAntesDeClassificar(double pontos, int inteiro, ClassificacaoRisco risco)
    {
        // Configuração isolada atribui o valor a um único componente para exercitar os limites do total.
        var premissas = PremissasScore.Versao1 with
        {
            MaximoLotacao = (decimal)pontos, MaximoFinanceiro = 0,
            PontosVacina = 0, PontosCarrapato = 0, PontosVermifugo = 0, PontosModificador = 0,
            PontosRaca = 0, PontosPrazoIdeal = 0, PontosPrazoForaIdeal = 0, PontosPossePropria = 0, PontosRotacao = 0
        };
        var resultado = new CalculadoraScore(PremissasLotacao.Versao1, premissas).Calcular(Dados(), 1m, 0m);
        Assert.AreEqual(inteiro, resultado.Total);
        Assert.AreEqual(risco, resultado.Classificacao);
    }

    [TestMethod]
    public void TotalPermaneceLimitadoMesmoComPremissasPersonalizadas()
    {
        var premissas = PremissasScore.Versao1 with { MaximoLotacao = 100m };
        Assert.AreEqual(100, new CalculadoraScore(PremissasLotacao.Versao1, premissas).Calcular(Dados(), 1m, 0.2m).Total);
    }

    [TestMethod]
    public void RejeitaDivisoresInvalidosNasPremissas()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CalculadoraScore(PremissasLotacao.Versao1, PremissasScore.Versao1 with { OcupacaoMinimaIdeal = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CalculadoraScore(PremissasLotacao.Versao1, PremissasScore.Versao1 with { MargemSemPontos = 0.2m }));
        Assert.Throws<ArgumentOutOfRangeException>(() => calculadora.Calcular(Dados(), -1, 0));
    }

    [TestMethod]
    public void SanitariosAusentesNaoRecebemPontos()
    {
        var dados = new DadosSimulacao(new(100, TipoPastagem.Verao, 100, 450, false, null),
            TipoPosse.Arrendado, 0, true, 10, 1, false, new(false, false, false, false));
        var resultado = calculadora.Calcular(dados, 2m, -0.1m);
        Assert.AreEqual(1, resultado.Total);
        Assert.AreEqual(0m, resultado.Componentes.Single(c => c.Nome == "Sanidade").Pontos);
    }
}
