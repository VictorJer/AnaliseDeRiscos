using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Rebanhos;
using ScoreRebanho.Domain.Simulacoes;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Infrastructure.Tests;

[TestClass]
public sealed class RepositorioRebanhosTests
{
    private static readonly DateTimeOffset Data = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    private static AnaliseCompletaDto Analise(int dias = 180) =>
        new CalcularScoreRebanho(TimeProvider.System).Executar(new()
        {
            AreaHectares = 100, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
            PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10,
            DiasPastagem = dias, VacinaCarbunculo = true, ControleCarrapato = true, Vermifugo = true
        }).Value with { CalculadaEmUtc = Data.AddDays(dias - 180) };

    [TestMethod]
    public async Task CriacaoSalvaCadastroEHistoricoCompletoSemRecalcularNaConsulta()
    {
        await using var banco = await BancoTeste.CriarAsync();
        RebanhoGravadoDto criado;
        var analise = Analise();
        await using (var escopo = banco.Servicos.CreateAsyncScope())
            criado = await escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
                .CriarAsync("autor-a", new("Lote", "Fazenda"), analise);

        await using var leitura = banco.Servicos.CreateAsyncScope();
        var rebanho = await leitura.ServiceProvider.GetRequiredService<IRepositorioRebanhos>().ConsultarAsync(criado.Id, "autor-a");
        Assert.IsNotNull(rebanho);
        Assert.AreEqual("Lote", rebanho.Nome);
        Assert.AreEqual(criado.Versao, rebanho.Versao);
        Assert.AreEqual(JsonSerializer.Serialize(analise), JsonSerializer.Serialize(rebanho.Analise));
        var historico = await leitura.ServiceProvider.GetRequiredService<IRepositorioScores>()
            .ConsultarDoAutorAsync(criado.UltimoScoreId, "autor-a");
        Assert.IsNotNull(historico);
        Assert.AreEqual(criado.Id, historico.RebanhoOrigemId);
        Assert.AreEqual("Lote", historico.NomeRebanho);
        Assert.AreEqual("Fazenda", historico.Propriedade);
        Assert.AreEqual(Data, historico.CalculadoEmUtc);
        Assert.AreEqual(JsonSerializer.Serialize(analise), JsonSerializer.Serialize(historico.Analise));
        Assert.AreEqual(1, await ContarHistoricosAsync(leitura.ServiceProvider));
    }

    [TestMethod]
    public async Task AtualizacaoCriaNovoHistoricoEPreservaOAnterior()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var original = Analise();
        var criado = await repositorio.CriarAsync("autor-a", new("Antes", "Fazenda"), original);
        var nova = Analise(200);
        var atualizacao = await repositorio.AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Depois", "Outra"), nova);
        Assert.AreEqual(EstadoGravacaoRebanho.Sucesso, atualizacao.Estado);
        Assert.IsNotNull(atualizacao.Rebanho);
        Assert.AreNotEqual(criado.Versao, atualizacao.Rebanho.Versao);
        Assert.AreNotEqual(criado.UltimoScoreId, atualizacao.Rebanho.UltimoScoreId);
        var rebanho = await repositorio.ConsultarAsync(criado.Id, "autor-a");
        Assert.IsNotNull(rebanho);
        Assert.AreEqual("Depois", rebanho.Nome);
        Assert.AreEqual(original.CalculadaEmUtc, rebanho.CriadoEmUtc);
        Assert.AreEqual(nova.CalculadaEmUtc, rebanho.AtualizadoEmUtc);
        Assert.AreEqual(JsonSerializer.Serialize(nova), JsonSerializer.Serialize(rebanho.Analise));
        var antigo = await escopo.ServiceProvider.GetRequiredService<IRepositorioScores>().ConsultarDoAutorAsync(criado.UltimoScoreId, "autor-a");
        Assert.IsNotNull(antigo);
        Assert.AreEqual("Antes", antigo.NomeRebanho);
        Assert.AreEqual(JsonSerializer.Serialize(original), JsonSerializer.Serialize(antigo.Analise));
        Assert.AreEqual(2, await ContarHistoricosAsync(escopo.ServiceProvider));
    }

    [TestMethod]
    public async Task ExclusaoRemoveCadastroEMantemTodosOsHistoricos()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var criado = await repositorio.CriarAsync("autor-a", new("Lote", "Fazenda"), Analise());
        var atualizado = await repositorio.AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Novo", "Fazenda"), Analise(200));
        Assert.IsNotNull(atualizado.Rebanho);
        Assert.AreEqual(EstadoGravacaoRebanho.Sucesso,
            await repositorio.ExcluirAsync(criado.Id, "autor-a", atualizado.Rebanho.Versao));
        Assert.IsNull(await repositorio.ConsultarAsync(criado.Id, "autor-a"));
        Assert.AreEqual(0, (await repositorio.ListarAsync("autor-a", 1)).Total);
        var historico = escopo.ServiceProvider.GetRequiredService<IRepositorioScores>();
        Assert.IsNotNull(await historico.ConsultarDoAutorAsync(criado.UltimoScoreId, "autor-a"));
        Assert.IsNotNull(await historico.ConsultarDoAutorAsync(atualizado.Rebanho.UltimoScoreId, "autor-a"));
        Assert.AreEqual(2, await ContarHistoricosAsync(escopo.ServiceProvider));
    }

    [TestMethod]
    public async Task AutorDiferenteNaoConsultaEditaOuExcluiNemObtemHistorico()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var criado = await repositorio.CriarAsync("autor-a", new("Lote", "Fazenda"), Analise());
        Assert.IsNull(await repositorio.ConsultarAsync(criado.Id, "autor-b"));
        Assert.AreEqual(EstadoGravacaoRebanho.NaoEncontrado,
            (await repositorio.AtualizarAsync(criado.Id, "autor-b", criado.Versao, new("Ataque", "Outra"), Analise())).Estado);
        Assert.AreEqual(EstadoGravacaoRebanho.NaoEncontrado,
            await repositorio.ExcluirAsync(criado.Id, "autor-b", criado.Versao));
        Assert.AreEqual(0, (await repositorio.ListarAsync("autor-b", 1)).Total);
        Assert.IsNull(await escopo.ServiceProvider.GetRequiredService<IRepositorioScores>().ConsultarDoAutorAsync(criado.UltimoScoreId, "autor-b"));
        Assert.AreEqual(1, await ContarHistoricosAsync(escopo.ServiceProvider));
    }

    [TestMethod]
    public async Task PaginaTemVinteRegistrosOrdenadosPorAtualizacaoENomesRepetidosSaoPermitidos()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var ids = new List<Guid>();
        var analise = Analise();
        for (var indice = 0; indice < 21; indice++)
        {
            var criado = await repositorio.CriarAsync("autor-a", new("Repetido", "Fazenda"),
                analise with { CalculadaEmUtc = Data.AddMinutes(indice) });
            ids.Add(criado.Id);
        }
        await repositorio.CriarAsync("autor-b", new("Outro autor", "Fazenda"), analise);
        var pagina1 = await repositorio.ListarAsync("autor-a", 1);
        var pagina2 = await repositorio.ListarAsync("autor-a", 2);
        Assert.AreEqual(21, pagina1.Total);
        Assert.AreEqual(20, pagina1.TamanhoPagina);
        Assert.HasCount(20, pagina1.Itens);
        Assert.HasCount(1, pagina2.Itens);
        CollectionAssert.AreEqual(ids.AsEnumerable().Reverse().Take(20).ToArray(), pagina1.Itens.Select(i => i.Id).ToArray());
        Assert.AreEqual(ids[0], pagina2.Itens[0].Id);
        Assert.IsEmpty((await repositorio.ListarAsync("autor-a", int.MaxValue)).Itens);
    }

    [TestMethod]
    public async Task VersaoDesatualizadaRecusaAtualizacaoEExclusaoSemCriarHistorico()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        var criado = await repositorio.CriarAsync("autor-a", new("Lote", "Fazenda"), Analise());
        await repositorio.AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Atual", "Fazenda"), Analise(200));
        var conflito = await repositorio.AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Antigo", "Fazenda"), Analise(201));
        Assert.AreEqual(EstadoGravacaoRebanho.Conflito, conflito.Estado);
        Assert.IsNull(conflito.Rebanho);
        Assert.AreEqual(EstadoGravacaoRebanho.Conflito, await repositorio.ExcluirAsync(criado.Id, "autor-a", criado.Versao));
        Assert.AreEqual(2, await ContarHistoricosAsync(escopo.ServiceProvider));
        Assert.AreEqual("Atual", (await repositorio.ConsultarAsync(criado.Id, "autor-a"))!.Nome);
    }

    [TestMethod]
    public async Task ConcorrenciaEntreContextosRecusaSegundoEscritorEFazRollbackDoSeuScore()
    {
        await using var banco = await BancoTeste.CriarAsync();
        RebanhoGravadoDto criado;
        await using (var criacao = banco.Servicos.CreateAsyncScope())
            criado = await criacao.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
                .CriarAsync("autor-a", new("Lote", "Fazenda"), Analise());
        await using var primeiro = banco.Servicos.CreateAsyncScope();
        await using var segundo = banco.Servicos.CreateAsyncScope();
        await primeiro.ServiceProvider.GetRequiredService<ContasDbContext>().Set<Rebanho>().SingleAsync();
        await segundo.ServiceProvider.GetRequiredService<ContasDbContext>().Set<Rebanho>().SingleAsync();
        var vencedor = await primeiro.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
            .AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Vencedor", "Fazenda"), Analise(200));
        var perdedor = await segundo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
            .AtualizarAsync(criado.Id, "autor-a", criado.Versao, new("Perdedor", "Fazenda"), Analise(201));
        Assert.AreEqual(EstadoGravacaoRebanho.Sucesso, vencedor.Estado);
        Assert.AreEqual(EstadoGravacaoRebanho.Conflito, perdedor.Estado);
        Assert.AreEqual(2, await ContarHistoricosAsync(segundo.ServiceProvider));
        var repositorio = segundo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
        await repositorio.CriarAsync("autor-a", new("Outro", "Fazenda"), Analise());
        Assert.AreEqual("Vencedor", (await repositorio.ConsultarAsync(criado.Id, "autor-a"))!.Nome);
        Assert.AreEqual(3, await ContarHistoricosAsync(segundo.ServiceProvider));
    }

    [TestMethod]
    public async Task FalhaAoInserirCadastroNaoDeixaHistoricoOrfao()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using (var escopo = banco.Servicos.CreateAsyncScope())
        {
            var contexto = escopo.ServiceProvider.GetRequiredService<ContasDbContext>();
            await contexto.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER FalhaInsercao BEFORE INSERT ON Rebanhos
                BEGIN SELECT RAISE(ABORT, 'Falha simulada no teste'); END;
                """);
            await Assert.ThrowsAsync<DbUpdateException>(() => escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
                .CriarAsync("autor-a", new("Lote", "Fazenda"), Analise()));
        }
        await using var leitura = banco.Servicos.CreateAsyncScope();
        Assert.AreEqual(0, await ContarHistoricosAsync(leitura.ServiceProvider));
        Assert.AreEqual(0, (await leitura.ServiceProvider.GetRequiredService<IRepositorioRebanhos>().ListarAsync("autor-a", 1)).Total);
    }

    [TestMethod]
    public async Task FalhaAoAtualizarCadastroNaoAlteraUltimoResultadoNemCriaHistorico()
    {
        await using var banco = await BancoTeste.CriarAsync();
        RebanhoGravadoDto criado;
        await using (var escopo = banco.Servicos.CreateAsyncScope())
        {
            var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>();
            criado = await repositorio.CriarAsync("autor-a", new("Original", "Fazenda"), Analise());
            await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER FalhaAtualizacao BEFORE UPDATE ON Rebanhos
                BEGIN SELECT RAISE(ABORT, 'Falha simulada no teste'); END;
                """);
            await Assert.ThrowsAsync<DbUpdateException>(() => repositorio.AtualizarAsync(
                criado.Id, "autor-a", criado.Versao, new("Falha", "Fazenda"), Analise(200)));
        }
        await using var leitura = banco.Servicos.CreateAsyncScope();
        var rebanho = await leitura.ServiceProvider.GetRequiredService<IRepositorioRebanhos>().ConsultarAsync(criado.Id, "autor-a");
        Assert.IsNotNull(rebanho);
        Assert.AreEqual("Original", rebanho.Nome);
        Assert.AreEqual(criado.Versao, rebanho.Versao);
        Assert.AreEqual(criado.UltimoScoreId, rebanho.UltimoScoreId);
        Assert.AreEqual(1, await ContarHistoricosAsync(leitura.ServiceProvider));
    }

    [TestMethod]
    public async Task HistoricoAvulsoNaoCriaRebanhoEPreservaSnapshot()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        var historico = escopo.ServiceProvider.GetRequiredService<IRepositorioScores>();
        var analise = Analise();
        var id = await historico.SalvarAvulsoAsync("autor-a", analise);
        var salvo = await historico.ConsultarDoAutorAsync(id, "autor-a");
        Assert.IsNotNull(salvo);
        Assert.IsNull(salvo.RebanhoOrigemId);
        Assert.IsNull(salvo.NomeRebanho);
        Assert.AreEqual(JsonSerializer.Serialize(analise), JsonSerializer.Serialize(salvo.Analise));
        Assert.AreEqual(0, (await escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>().ListarAsync("autor-a", 1)).Total);
    }

    [TestMethod]
    public async Task BancoImpedeAlteracaoEExclusaoDiretaDoHistorico()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        await escopo.ServiceProvider.GetRequiredService<IRepositorioScores>().SalvarAvulsoAsync("autor-a", Analise());
        var contexto = escopo.ServiceProvider.GetRequiredService<ContasDbContext>();
        await Assert.ThrowsAsync<SqliteException>(() => contexto.Database.ExecuteSqlRawAsync("UPDATE ScoresHistoricos SET ScoreTotal = 0"));
        await Assert.ThrowsAsync<SqliteException>(() => contexto.Database.ExecuteSqlRawAsync("DELETE FROM ScoresHistoricos"));
        Assert.AreEqual(1, await ContarHistoricosAsync(escopo.ServiceProvider));
    }

    [TestMethod]
    public async Task CancelamentoNaoSalvaCadastroOuHistorico()
    {
        await using var banco = await BancoTeste.CriarAsync();
        await using var escopo = banco.Servicos.CreateAsyncScope();
        using var cancelamento = new CancellationTokenSource();
        cancelamento.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>()
            .CriarAsync("autor-a", new("Lote", "Fazenda"), Analise(), cancelamento.Token));
        await using var leitura = banco.Servicos.CreateAsyncScope();
        Assert.AreEqual(0, await ContarHistoricosAsync(leitura.ServiceProvider));
    }

    [TestMethod]
    public async Task MigrationAtualizaBancoIdentityExistentePreservandoContas()
    {
        await using var contexto = new ContasDbContextFactory().CreateDbContext([]);
        await contexto.Database.OpenConnectionAsync();
        var migrador = contexto.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        await migrador.MigrateAsync("00000000000000_CreateIdentitySchema");
        contexto.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser { Id = "preservado", UserName = "existente@example.com" });
        await contexto.SaveChangesAsync();
        await contexto.Database.MigrateAsync();
        Assert.IsNotNull(await contexto.Users.SingleOrDefaultAsync(u => u.Id == "preservado"));
        Assert.HasCount(2, (await contexto.Database.GetAppliedMigrationsAsync()).ToArray());
        Assert.IsFalse(contexto.Database.HasPendingModelChanges());
    }
    private static Task<int> ContarHistoricosAsync(IServiceProvider servicos) =>
        servicos.GetRequiredService<ContasDbContext>().Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM ScoresHistoricos").SingleAsync();
}
