using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Infrastructure.Tests;

[TestClass]
public sealed class ConsultaScoresAdministrativosTests
{
    private static AnaliseCompletaDto Analise() => new CalcularScoreRebanho(TimeProvider.System).Executar(new()
    {
        AreaHectares = 100, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
        PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10, DiasPastagem = 180
    }).Value with { CalculadaEmUtc = DateTimeOffset.UnixEpoch };

    [TestMethod]
    public async Task ListagemGlobalPaginaEmVinteEOrdenaPorDataComDesempateEstavel()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var scores = escopo.ServiceProvider.GetRequiredService<IRepositorioScores>();
        var consulta = escopo.ServiceProvider.GetRequiredService<IConsultaScoresAdministrativos>();
        var analise = Analise();
        var esperados = new List<(Guid Id, DateTimeOffset Data)>();
        for (var i = 0; i < 23; i++)
        {
            var entrada = analise with { CalculadaEmUtc = analise.CalculadaEmUtc.AddMinutes(i / 2) };
            esperados.Add((await scores.SalvarAvulsoAsync(i % 2 == 0 ? "autor-a" : "autor-b", entrada), entrada.CalculadaEmUtc));
        }
        var primeira = await consulta.ListarAsync(1);
        var segunda = await consulta.ListarAsync(2);
        Assert.AreEqual(23, primeira.Total);
        Assert.AreEqual(20, primeira.TamanhoPagina);
        Assert.HasCount(20, primeira.Itens);
        Assert.HasCount(3, segunda.Itens);
        CollectionAssert.AreEqual(esperados.OrderByDescending(e => e.Data).ThenByDescending(e => e.Id.ToString(), StringComparer.Ordinal)
            .Select(e => e.Id).ToArray(), primeira.Itens.Concat(segunda.Itens).Select(e => e.Id).ToArray());
        Assert.IsTrue(primeira.Itens.Any(i => i.AutorId == "autor-a"));
        Assert.IsTrue(primeira.Itens.Any(i => i.AutorId == "autor-b"));
        Assert.IsTrue(primeira.Itens.All(i => i.AutorEmail == i.AutorId + "@example.com"));
        Assert.AreEqual(analise.Premissas.Versao, primeira.Itens[0].VersaoPremissas);
        Assert.IsEmpty((await consulta.ListarAsync(int.MaxValue)).Itens);
    }

    [TestMethod]
    public async Task RelatorioPreservaSnapshotAposAtualizacaoEExclusaoDoCadastro()
    {
        await using var banco = await BancoTeste.CriarAsync();
        var analise = Analise();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var rebanhos = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var consulta = escopo.ServiceProvider.GetRequiredService<IConsultaScoresAdministrativos>();
        var criado = await rebanhos.CriarAsync("autor-a", new("Original", "Fazenda"), analise);
        var atualizado = await rebanhos.AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Novo", "Outra fazenda"),
            analise with { CalculadaEmUtc = analise.CalculadaEmUtc.AddDays(1) });
        Assert.IsNotNull(atualizado.Rebanho);
        await rebanhos.ExcluirAsync(criado.Id, "autor-a", atualizado.Rebanho.Versao);
        var historico = await consulta.ConsultarAsync(criado.UltimoScoreId);
        Assert.IsNotNull(historico);
        Assert.AreEqual("Original", historico.NomeRebanho);
        Assert.AreEqual("Fazenda", historico.Propriedade);
        Assert.AreEqual("autor-a", historico.AutorId);
        Assert.AreEqual(JsonSerializer.Serialize(analise), JsonSerializer.Serialize(historico.Analise));
        Assert.AreEqual(2, (await consulta.ListarAsync(1)).Total);
    }

    [TestMethod]
    public async Task ConsultaAusenteEPaginaVaziaNaoCriamRegistrosECancelamentoEPropagado()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var consulta = escopo.ServiceProvider.GetRequiredService<IConsultaScoresAdministrativos>();
        Assert.IsNull(await consulta.ConsultarAsync(Guid.NewGuid()));
        Assert.IsEmpty((await consulta.ListarAsync(1)).Itens);
        using var cancelamento = new CancellationTokenSource();
        cancelamento.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => consulta.ListarAsync(1, cancelamento.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => consulta.ConsultarAsync(Guid.NewGuid(), cancelamento.Token));
    }
}
