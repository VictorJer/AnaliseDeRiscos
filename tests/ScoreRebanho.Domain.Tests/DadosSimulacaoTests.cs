using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class DadosSimulacaoTests
{
    private static DadosLotacao Lotacao => new(10m, TipoPastagem.Verao, 10, 300m, false, null);
    private static ManejoSanitario Manejo => new(true, true, true, false);

    [TestMethod]
    public void PreservaEntradasValidasSemArredondar()
    {
        var dados = new DadosSimulacao(Lotacao, TipoPosse.Arrendado, 123.456m, false, 9.876m, 180, true, Manejo);
        Assert.AreEqual(123.456m, dados.ArrendamentoMensalReais);
        Assert.AreEqual(9.876m, dados.PrecoCompraPorKgReais);
        Assert.AreEqual(180, dados.DiasPastagem);
        Assert.IsFalse(dados.PastoPronto);
        Assert.IsTrue(dados.RacaPuraCorte);
        Assert.AreEqual(Manejo, dados.Manejo);
        Assert.AreEqual(Lotacao, dados.Lotacao);
    }

    [TestMethod]
    public void ArrendadoExigeValorMasPermiteZero()
    {
        Assert.Throws<ArgumentException>(() => new DadosSimulacao(Lotacao, TipoPosse.Arrendado, null, true, 10, 1, false, Manejo));
        var dados = new DadosSimulacao(Lotacao, TipoPosse.Arrendado, 0m, true, 10, 1, false, Manejo);
        Assert.AreEqual(0m, dados.ArrendamentoMensalReais);
    }

    [TestMethod]
    public void TerrenoProprioNaoTemDespesaDeArrendamento()
    {
        var dados = new DadosSimulacao(Lotacao, TipoPosse.Proprio, 1000m, true, 10, 1, false, Manejo);
        Assert.AreEqual(0m, dados.ArrendamentoMensalReais);
        Assert.AreEqual(0m, new DadosSimulacao(Lotacao, TipoPosse.Proprio, null, true, 10, 1, false, Manejo).ArrendamentoMensalReais);
    }

    [TestMethod]
    public void RejeitaArrendamentoNegativoEPosseInvalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DadosSimulacao(Lotacao, TipoPosse.Arrendado, -1, true, 10, 1, false, Manejo));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DadosSimulacao(Lotacao, (TipoPosse)99, null, true, 10, 1, false, Manejo));
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(-1, 1)]
    [DataRow(10, 0)]
    [DataRow(10, -1)]
    public void PrecoEDiasDevemSerPositivos(int preco, int dias)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DadosSimulacao(Lotacao, TipoPosse.Proprio, null, true, preco, dias, false, Manejo));
    }

    [TestMethod]
    public void RejeitaLotacaoOuManejoAusentes()
    {
        Assert.Throws<ArgumentNullException>(() => new DadosSimulacao(null!, TipoPosse.Proprio, null, true, 10, 1, false, Manejo));
        Assert.Throws<ArgumentNullException>(() => new DadosSimulacao(Lotacao, TipoPosse.Proprio, null, true, 10, 1, false, null!));
    }
}
