using System.Text.Json;
using System.Text.Json.Nodes;
using FluentResults;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Relatorios;

namespace ScoreRebanho.Application.Tests.Rebanhos;

[TestClass]
public sealed class CasosUsoRebanhosTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task AmbosPerfisCriamAtualizamConsultamEExcluemComMesmoMotor(string perfil)
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", perfil);
        var criado = await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada);
        Assert.IsTrue(criado.IsSuccess);
        var consulta = await cenario.Consultar.ExecutarAsync(new(criado.Value.Id));
        Assert.IsTrue(consulta.IsSuccess);
        var atualizado = await cenario.Atualizar.ExecutarAsync(new(criado.Value.Id, criado.Value.Versao,
            "Novo lote", "Outra fazenda", CenarioRebanhos.Entrada.Entradas with { DiasPastagem = 300 }));
        Assert.IsTrue(atualizado.IsSuccess);
        Assert.AreNotEqual(criado.Value.UltimoScoreId, atualizado.Value.UltimoScoreId);
        Assert.AreEqual(2, cenario.Relogio.Chamadas);
        Assert.IsTrue((await cenario.Excluir.ExecutarAsync(new(atualizado.Value.Id, atualizado.Value.Versao))).IsSuccess);
        var antigo = await cenario.Historico.ExecutarAsync(new(criado.Value.UltimoScoreId));
        Assert.IsTrue(antigo.IsSuccess);
        Assert.AreEqual("Lote", antigo.Value.NomeRebanho);
    }

    [TestMethod]
    public async Task PerfisDiferentesComMesmasEntradasGeramResultadosCompletosIguais()
    {
        var cenario = new CenarioRebanhos();
        var pecuarista = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        cenario.Usuario.Conta = new("autor-b", PerfisNegocio.Cooperativa);
        var cooperativa = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var primeiro = await cenario.Persistencia.ConsultarAsync(pecuarista.Id, "autor-a");
        var segundo = await cenario.Persistencia.ConsultarAsync(cooperativa.Id, "autor-b");
        Assert.AreEqual(JsonSerializer.Serialize(primeiro!.Analise), JsonSerializer.Serialize(segundo!.Analise));
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task OutroUsuarioNaoListaConsultaEditaExcluiOuLeHistorico(string perfil)
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        cenario.Usuario.Conta = new("autor-b", perfil);
        Codigo((await cenario.Consultar.ExecutarAsync(new(criado.Id))).Errors, "NAO_ENCONTRADO");
        Codigo((await cenario.Edicao.ExecutarAsync(new(criado.Id))).Errors, "NAO_ENCONTRADO");
        Codigo((await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao, "", "", null!))).Errors, "NAO_ENCONTRADO");
        Codigo((await cenario.Excluir.ExecutarAsync(new(criado.Id, criado.Versao))).Errors, "NAO_ENCONTRADO");
        Codigo((await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId))).Errors, "NAO_ENCONTRADO");
        Assert.IsEmpty((await cenario.Listar.ExecutarAsync(new())).Value.Itens);
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
        Assert.AreEqual(1, cenario.Persistencia.Gravacoes);
    }

    [TestMethod]
    public async Task SemAutenticacaoNenhumCasoDeUsoAcessaPersistencia()
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = null;
        Codigo((await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Listar.ExecutarAsync(new())).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Consultar.ExecutarAsync(new(Guid.NewGuid()))).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Edicao.ExecutarAsync(new(Guid.NewGuid()))).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Atualizar.ExecutarAsync(new(Guid.NewGuid(), Guid.NewGuid(), "", "", null!))).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Excluir.ExecutarAsync(new(Guid.NewGuid(), Guid.NewGuid()))).Errors, "NAO_AUTENTICADO");
        Codigo((await cenario.Historico.ExecutarAsync(new(Guid.NewGuid()))).Errors, "NAO_AUTENTICADO");
        Assert.AreEqual(0, cenario.Persistencia.Chamadas);
        Assert.AreEqual(0, cenario.Relogio.Chamadas);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("Administrador")]
    [DataRow("perfil-inventado")]
    public async Task PerfilAusenteOuDesconhecidoNaoRecebePrivilegios(string? perfil)
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", perfil);
        Codigo((await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Errors, "PERFIL_INVALIDO");
        Codigo((await cenario.Listar.ExecutarAsync(new())).Errors, "PERFIL_INVALIDO");
        Codigo((await cenario.Consultar.ExecutarAsync(new(Guid.NewGuid()))).Errors, "PERFIL_INVALIDO");
        Codigo((await cenario.Historico.ExecutarAsync(new(Guid.NewGuid()))).Errors, "PERFIL_INVALIDO");
        Assert.AreEqual(0, cenario.Persistencia.Chamadas);
    }

    [TestMethod]
    public async Task IdentificacaoESimulacaoInvalidasNaoSaoSalvas()
    {
        var cenario = new CenarioRebanhos();
        foreach (var entrada in new[]
        {
            CenarioRebanhos.Entrada with { Nome = "" },
            CenarioRebanhos.Entrada with { Nome = new string('N', 101) },
            CenarioRebanhos.Entrada with { Propriedade = "" },
            CenarioRebanhos.Entrada with { Propriedade = new string('P', 151) },
            CenarioRebanhos.Entrada with { Entradas = null! },
            CenarioRebanhos.Entrada with { Entradas = CenarioRebanhos.Entrada.Entradas with { AreaHectares = 0 } }
        })
            Codigo((await cenario.Criar.ExecutarAsync(entrada)).Errors, "ENTRADA_INVALIDA");
        Assert.AreEqual(0, cenario.Persistencia.Gravacoes);
    }

    [TestMethod]
    public async Task VersaoObsoletaNaoRecalculaNemGrava()
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        Codigo((await cenario.Atualizar.ExecutarAsync(new(criado.Id, Guid.NewGuid(), "Lote", "Fazenda",
            CenarioRebanhos.Entrada.Entradas))).Errors, "CONFLITO");
        Codigo((await cenario.Excluir.ExecutarAsync(new(criado.Id, Guid.NewGuid()))).Errors, "CONFLITO");
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
        Assert.AreEqual(1, cenario.Persistencia.Gravacoes);
    }

    [TestMethod]
    [DataRow(EstadoGravacaoRebanho.Conflito, "CONFLITO")]
    [DataRow(EstadoGravacaoRebanho.NaoEncontrado, "NAO_ENCONTRADO")]
    public async Task CorridaEntreConsultaEGravacaoRetornaFalhaEsperada(EstadoGravacaoRebanho estado, string codigo)
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        cenario.Persistencia.RespostaAtualizacao = new(estado, null);
        cenario.Persistencia.RespostaExclusao = estado;
        Codigo((await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao, "Lote", "Fazenda",
            CenarioRebanhos.Entrada.Entradas))).Errors, codigo);
        Codigo((await cenario.Excluir.ExecutarAsync(new(criado.Id, criado.Versao))).Errors, codigo);
    }

    [TestMethod]
    public async Task EdicaoRetornaEntradasSemRelatorioERelatorioNaoRecalcula()
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var edicao = (await cenario.Edicao.ExecutarAsync(new(criado.Id))).Value;
        Assert.AreEqual(criado.Versao, edicao.Versao);
        Assert.AreEqual(CenarioRebanhos.Entrada.Entradas.QuantidadeAnimais, edicao.Entradas.QuantidadeAnimais);
        await cenario.Consultar.ExecutarAsync(new(criado.Id));
        await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId));
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task AutoriaPerfilEScoreAdicionaisNoPayloadSaoIgnorados()
    {
        var cenario = new CenarioRebanhos();
        var json = JsonSerializer.SerializeToNode(CenarioRebanhos.Entrada)!;
        json["AutorId"] = "autor-b";
        json["PerfilNegocio"] = "Cooperativa";
        json["Score"] = 100;
        var criado = (await cenario.Criar.ExecutarAsync(json.Deserialize<CriarRebanhoDto>()!)).Value;
        Assert.AreEqual("autor-a", cenario.Persistencia.UltimoAutor);
        Assert.IsNull(await cenario.Persistencia.ConsultarAsync(criado.Id, "autor-b"));
        var consulta = (await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value;
        Assert.IsInstanceOfType<ResumoPecuaristaDto>(consulta.Relatorio);
        Assert.IsLessThan(100, ((ResumoPecuaristaDto)consulta.Relatorio).Score);
    }

    [TestMethod]
    public async Task PaginaInvalidaFalhaEPropagaCancelamentoParaPersistencia()
    {
        var cenario = new CenarioRebanhos();
        Codigo((await cenario.Listar.ExecutarAsync(new(0))).Errors, "ENTRADA_INVALIDA");
        Assert.AreEqual(0, cenario.Persistencia.Chamadas);
        using var cancelamento = new CancellationTokenSource();
        var criado = await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada, cancelamento.Token);
        Assert.IsTrue(criado.IsSuccess);
        Assert.AreEqual(cancelamento.Token, cenario.Persistencia.UltimoToken);
        cancelamento.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => cenario.Listar.ExecutarAsync(new(), cancelamento.Token));
    }

    [TestMethod]
    public async Task FalhasInesperadasNaoSaoConvertidasEmSucessoOuErroDeValidacao()
    {
        var cenario = new CenarioRebanhos();
        cenario.Persistencia.FalharCriacao = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada));
        cenario.Persistencia.FalharCriacao = false;
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        cenario.Persistencia.RespostaAtualizacao = new(EstadoGravacaoRebanho.Sucesso, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => cenario.Atualizar.ExecutarAsync(
            new(criado.Id, criado.Versao, "Lote", "Fazenda", CenarioRebanhos.Entrada.Entradas)));
    }

    [TestMethod]
    public async Task AtualizacaoInvalidaNaoSubstituiAnaliseNemGeraGravacao()
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        Codigo((await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao, "Lote", "Fazenda",
            CenarioRebanhos.Entrada.Entradas with { AreaHectares = 0 }))).Errors, "ENTRADA_INVALIDA");
        Codigo((await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao, "Lote", "Fazenda",
            CenarioRebanhos.Entrada.Entradas with { PrecoCompraPorKgReais = decimal.MaxValue }))).Errors, "LIMITE_NUMERICO");
        Assert.AreEqual(1, cenario.Persistencia.Gravacoes);
        Assert.AreEqual(criado.UltimoScoreId, (await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value.UltimoScoreId);
    }

    private static void Codigo(IEnumerable<IError> erros, string esperado)
    {
        var erro = erros.First();
        Assert.AreEqual(esperado, erro.Metadata["Codigo"]);
    }
}
