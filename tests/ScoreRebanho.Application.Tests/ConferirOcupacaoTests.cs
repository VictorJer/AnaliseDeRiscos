using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Application.Tests.Rebanhos;
using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Pastagens;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public sealed class ConferirOcupacaoTests
{
    [TestMethod]
    public async Task ConferenciaUsaMesmasPremissasDoMotorSemCalcularScore()
    {
        var cenario = new CenarioRebanhos();
        var resultado = await new ConferirOcupacao(cenario.Usuario, cenario.Calcular).ExecutarAsync(CenarioRebanhos.Entrada.Entradas);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(1m, resultado.Value.Lotacao.OcupacaoDecimal);
        Assert.AreEqual("1.0", resultado.Value.VersaoPremissas);
        Assert.AreEqual(0, cenario.Relogio.Chamadas);
        Assert.AreEqual(0, cenario.Persistencia.Gravacoes);
    }

    [TestMethod]
    public async Task ConferenciaRespeitaPremissasPersonalizadas()
    {
        var cenario = new CenarioRebanhos();
        var p = PremissasAnalise.Versao1;
        var lotacao = new PremissasLotacao("1.0", 450m, 2m, 1.2m, 0.15m, 1.6m, 3);
        var motor = new CalcularScoreRebanho(new(lotacao, p.Producao, p.Custos, p.Financeiro, p.Score), TimeProvider.System);
        var resultado = await new ConferirOcupacao(cenario.Usuario, motor).ExecutarAsync(CenarioRebanhos.Entrada.Entradas);
        Assert.AreEqual(0.5m, resultado.Value.Lotacao.OcupacaoDecimal);
    }

    [TestMethod]
    public async Task ConferenciaExigeUsuarioEValidaEntradas()
    {
        var cenario = new CenarioRebanhos();
        var conferir = new ConferirOcupacao(cenario.Usuario, cenario.Calcular);
        var invalido = await conferir.ExecutarAsync(CenarioRebanhos.Entrada.Entradas with { AreaHectares = 0 });
        Assert.IsTrue(invalido.IsFailed);
        Assert.AreEqual("ENTRADA_INVALIDA", invalido.Errors[0].Metadata["Codigo"]);
        cenario.Usuario.Conta = null;
        Assert.AreEqual("NAO_AUTENTICADO", (await conferir.ExecutarAsync(CenarioRebanhos.Entrada.Entradas)).Errors[0].Metadata["Codigo"]);
    }
}
