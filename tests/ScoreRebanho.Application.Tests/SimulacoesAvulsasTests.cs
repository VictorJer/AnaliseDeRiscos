using System.Text.Json;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Application.Tests.Rebanhos;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public sealed class SimulacoesAvulsasTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task GerarERecalcularPreservaOriginalSemCriarRebanho(string perfil)
    {
        var c = new CenarioRebanhos();
        c.Usuario.Conta = new("autor-a", perfil);
        var gerar = new GerarSimulacao(c.Usuario, c.Persistencia, c.Calcular);
        var edicao = new ConsultarEdicaoSimulacao(c.Usuario, c.Persistencia);
        var primeiro = await gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas));
        Assert.IsTrue(primeiro.IsSuccess);
        var original = await c.Persistencia.ConsultarDoAutorAsync(primeiro.Value.Id, "autor-a");
        Assert.IsNotNull(original);
        var snapshot = JsonSerializer.Serialize(original);
        Assert.IsNull(original.RebanhoOrigemId);
        Assert.IsNull(original.NomeRebanho);
        Assert.IsNull(original.Propriedade);
        var dados = await edicao.ExecutarAsync(new(primeiro.Value.Id));
        Assert.IsTrue(dados.IsSuccess);
        Assert.AreEqual(original.Analise.Entradas, dados.Value.Entradas);
        Assert.AreEqual(0m, dados.Value.Entradas.ArrendamentoMensalReais);
        var segundo = await gerar.ExecutarAsync(new(dados.Value.Entradas with { DiasPastagem = 300 }));
        Assert.IsTrue(segundo.IsSuccess);
        Assert.AreNotEqual(primeiro.Value.Id, segundo.Value.Id);
        Assert.AreEqual(snapshot, JsonSerializer.Serialize(await c.Persistencia.ConsultarDoAutorAsync(primeiro.Value.Id, "autor-a")));
        Assert.AreEqual(0, (await c.Listar.ExecutarAsync(new(1))).Value.Total);
        Assert.AreEqual(2, c.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task ContaOuEntradaInvalidaNaoCalculaNemPersiste()
    {
        var c = new CenarioRebanhos();
        var gerar = new GerarSimulacao(c.Usuario, c.Persistencia, c.Calcular);
        c.Usuario.Conta = null;
        var anonimo = await gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas));
        Assert.AreEqual("NAO_AUTENTICADO", anonimo.Errors[0].Metadata["Codigo"]);
        c.Usuario.Conta = new("a", "invalido");
        Assert.IsTrue((await gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas))).IsFailed);
        c.Usuario.Conta = new("a", PerfisNegocio.Pecuarista);
        Assert.IsTrue((await gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas with { AreaHectares = 0 }))).IsFailed);
        Assert.IsTrue((await gerar.ExecutarAsync(new(null!))).IsFailed);
        Assert.AreEqual(0, c.Persistencia.Chamadas);
        Assert.AreEqual(0, c.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task EdicaoRecusaOutroAutorEHistoricoDeRebanhoSemRecalcular()
    {
        var c = new CenarioRebanhos();
        var gerar = new GerarSimulacao(c.Usuario, c.Persistencia, c.Calcular);
        var edicao = new ConsultarEdicaoSimulacao(c.Usuario, c.Persistencia);
        var salvo = (await gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas))).Value;
        c.Usuario.Conta = new("outro", PerfisNegocio.Cooperativa);
        var alheio = await edicao.ExecutarAsync(new(salvo.Id));
        Assert.AreEqual("NAO_ENCONTRADO", alheio.Errors[0].Metadata["Codigo"]);
        c.Usuario.Conta = new("autor-a", PerfisNegocio.Pecuarista);
        var rebanho = (await c.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        Assert.IsTrue((await edicao.ExecutarAsync(new(rebanho.UltimoScoreId))).IsFailed);
        Assert.IsTrue((await edicao.ExecutarAsync(new(Guid.Empty))).IsFailed);
        Assert.AreEqual(2, c.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task CancelamentoNaoIniciaCalculoOuGravacao()
    {
        var c = new CenarioRebanhos();
        var gerar = new GerarSimulacao(c.Usuario, c.Persistencia, c.Calcular);
        var edicao = new ConsultarEdicaoSimulacao(c.Usuario, c.Persistencia);
        using var cancelamento = new CancellationTokenSource();
        cancelamento.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => gerar.ExecutarAsync(new(CenarioRebanhos.Entrada.Entradas), cancelamento.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => edicao.ExecutarAsync(new(Guid.NewGuid()), cancelamento.Token));
        Assert.AreEqual(0, c.Persistencia.Chamadas);
        Assert.AreEqual(0, c.Relogio.Chamadas);
    }
}
