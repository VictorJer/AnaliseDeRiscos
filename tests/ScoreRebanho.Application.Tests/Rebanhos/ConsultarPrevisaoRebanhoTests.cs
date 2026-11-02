using ScoreRebanho.Application.Clima;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Tests.Rebanhos;

[TestClass]
public sealed class ConsultarPrevisaoRebanhoTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task ConsultaSomentePontoSalvoSemRecalcularOuCriarHistorico(string perfil)
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", perfil);
        var id = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada with { Latitude = -27.8m, Longitude = -49.2m })).Value.Id;
        var cliente = new PrevisaoTeste();
        var resultado = await new ConsultarPrevisaoRebanho(cenario.Usuario, cenario.Persistencia, cliente).ExecutarAsync(new(id));
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(-27.8m, cliente.Latitude);
        Assert.AreEqual(-49.2m, cliente.Longitude);
        Assert.AreEqual(1, cenario.Persistencia.Gravacoes);
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task SemLocalizacaoOuSemAutoriaNaoConsultaFornecedor()
    {
        var cenario = new CenarioRebanhos();
        var id = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value.Id;
        var cliente = new PrevisaoTeste();
        var consulta = new ConsultarPrevisaoRebanho(cenario.Usuario, cenario.Persistencia, cliente);
        var semPonto = await consulta.ExecutarAsync(new(id));
        Assert.IsTrue(semPonto.IsFailed);
        Assert.AreEqual("SEM_LOCALIZACAO", semPonto.Errors[0].Metadata["Codigo"]);
        cenario.Usuario.Conta = new("autor-b", PerfisNegocio.Cooperativa, true);
        var terceiro = await consulta.ExecutarAsync(new(id));
        Assert.IsTrue(terceiro.IsFailed);
        Assert.AreEqual("NAO_ENCONTRADO", terceiro.Errors[0].Metadata["Codigo"]);
        cenario.Usuario.Conta = null;
        Assert.IsTrue((await consulta.ExecutarAsync(new(id))).IsFailed);
        Assert.AreEqual(0, cliente.Chamadas);
    }

    [TestMethod]
    public async Task IndisponibilidadeRetornaFalhaClaraSemAlterarRebanho()
    {
        var cenario = new CenarioRebanhos();
        var salvo = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada with { Latitude = 0, Longitude = 0 })).Value;
        var cliente = new PrevisaoTeste { Indisponivel = true };
        var resultado = await new ConsultarPrevisaoRebanho(cenario.Usuario, cenario.Persistencia, cliente).ExecutarAsync(new(salvo.Id));
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("PREVISAO_INDISPONIVEL", resultado.Errors[0].Metadata["Codigo"]);
        Assert.AreEqual(salvo.Versao, (await cenario.Edicao.ExecutarAsync(new(salvo.Id))).Value.Versao);
    }

    private sealed class PrevisaoTeste : IConsultaPrevisao
    {
        internal decimal Latitude { get; private set; }
        internal decimal Longitude { get; private set; }
        internal int Chamadas { get; private set; }
        internal bool Indisponivel { get; init; }
        public Task<PrevisaoDto?> ConsultarAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
        {
            Chamadas++; Latitude = latitude; Longitude = longitude;
            return Task.FromResult(Indisponivel ? null : new PrevisaoDto(DateTimeOffset.UtcNow, "America/Sao_Paulo", Array.Empty<DiaPrevisaoDto>()));
        }
    }
}
