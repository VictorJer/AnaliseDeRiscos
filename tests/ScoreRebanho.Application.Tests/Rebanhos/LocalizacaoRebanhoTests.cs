namespace ScoreRebanho.Application.Tests.Rebanhos;

[TestClass]
public sealed class LocalizacaoRebanhoTests
{
    [TestMethod]
    public async Task CriaConsultaERemoveLocalizacaoSemAlterarAnaliseAnterior()
    {
        var cenario = new CenarioRebanhos();
        var entrada = CenarioRebanhos.Entrada with { Latitude = -27.9m, Longitude = -49.8m };
        var criado = (await cenario.Criar.ExecutarAsync(entrada)).Value;
        var edicao = (await cenario.Edicao.ExecutarAsync(new(criado.Id))).Value;
        Assert.AreEqual(entrada.Latitude, edicao.Latitude);
        Assert.AreEqual(entrada.Longitude, edicao.Longitude);
        var atualizado = await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao,
            edicao.Nome, edicao.Propriedade, edicao.Entradas));
        Assert.IsTrue(atualizado.IsSuccess);
        Assert.IsNull((await cenario.Edicao.ExecutarAsync(new(criado.Id))).Value.Latitude);
        Assert.IsTrue((await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId))).IsSuccess);
    }

    [TestMethod]
    public async Task DadosInvalidosNaoCalculamNemGravam()
    {
        var cenario = new CenarioRebanhos();
        foreach (var entrada in new[]
        {
            CenarioRebanhos.Entrada with { Latitude = -27 },
            CenarioRebanhos.Entrada with { Longitude = -49 },
            CenarioRebanhos.Entrada with { Latitude = 91, Longitude = -49 },
            CenarioRebanhos.Entrada with { Latitude = -27, Longitude = -181 }
        })
            Assert.IsTrue((await cenario.Criar.ExecutarAsync(entrada)).IsFailed);
        Assert.AreEqual(0, cenario.Persistencia.Gravacoes);
        Assert.AreEqual(0, cenario.Relogio.Chamadas);
    }
}
