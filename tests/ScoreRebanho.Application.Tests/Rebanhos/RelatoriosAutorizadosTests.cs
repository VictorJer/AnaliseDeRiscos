using System.Text.Json;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Relatorios;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Tests.Rebanhos;

[TestClass]
public sealed class RelatoriosAutorizadosTests
{
    [TestMethod]
    public async Task PecuaristaRecebeItensEssenciaisSemDetalhesRestritosMesmoNaSerializacao()
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var completo = (await cenario.Persistencia.ConsultarAsync(criado.Id, "autor-a"))!.Analise;
        var relatorio = (await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value;
        Assert.IsInstanceOfType<ResumoPecuaristaDto>(relatorio.Relatorio);
        var resumo = (ResumoPecuaristaDto)relatorio.Relatorio;
        Assert.AreEqual("Lote", relatorio.Nome);
        Assert.AreEqual("Fazenda", relatorio.Propriedade);
        Assert.AreEqual(completo.CalculadaEmUtc, resumo.CalculadaEmUtc);
        Assert.AreEqual(completo.Score.Total, resumo.Score);
        Assert.AreEqual(completo.Score.Classificacao, resumo.Classificacao);
        Assert.AreEqual(completo.Producao.PesoVendaPorAnimalKg, resumo.PesoVendaPorAnimalKg);
        Assert.AreEqual(completo.Lotacao.CapacidadeUa, resumo.Capacidade.CapacidadeUa);
        Assert.AreEqual(completo.Lotacao.OcupacaoDecimal, resumo.Capacidade.OcupacaoDecimal);
        Assert.AreEqual(completo.Lotacao.QuantidadeMaximaAnimais, resumo.Capacidade.QuantidadeMaximaAnimais);
        Assert.AreEqual(completo.Custos.CustoTotalReais, resumo.Financeiro.CustoTotalReais);
        Assert.AreEqual(completo.Custos.CapitalInicialReais, resumo.Financeiro.CapitalInicialReais);
        Assert.AreEqual(completo.Financeiro.Referencia.ReceitaReais, resumo.Financeiro.ReceitaReais);
        Assert.AreEqual(completo.Financeiro.Referencia.LucroReais, resumo.Financeiro.LucroReais);
        Assert.AreEqual(completo.Financeiro.Referencia.MargemDecimal, resumo.Financeiro.MargemDecimal);
        Assert.AreEqual(completo.Financeiro.PrecoEquilibrioPorKgReais, resumo.Financeiro.PrecoEquilibrioPorKgReais);
        Assert.AreEqual(completo.Financeiro.Adverso.LucroReais, resumo.LucroCenarioAdversoReais);
        Assert.AreEqual(completo.Premissas.Versao, resumo.VersaoPremissas);
        StringAssert.Contains(resumo.Financeiro.ExplicacaoPrecoEquilibrio, "cobrir os custos");
        Assert.IsTrue(resumo.Avisos.Any(a => a.Contains("não garante lucro")));
        Assert.IsTrue(resumo.Avisos.Any(a => a.Contains("capital inicial não inclui")));
        var json = JsonSerializer.Serialize(relatorio);
        SemDetalhesRestritos(json);
        var recuperado = JsonSerializer.Deserialize<RelatorioRebanhoDto>(json);
        Assert.IsNotNull(recuperado);
        Assert.IsInstanceOfType<ResumoPecuaristaDto>(recuperado.Relatorio);
    }

    [TestMethod]
    public async Task CooperativaRecebeEntradasComponentesCustosPremissasEFormulas()
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", PerfisNegocio.Cooperativa);
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var relatorio = (await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value;
        Assert.IsInstanceOfType<RelatorioCooperativaDto>(relatorio.Relatorio);
        var analise = ((RelatorioCooperativaDto)relatorio.Relatorio).Analise;
        Assert.HasCount(4, analise.Score.Componentes);
        Assert.IsNotEmpty(analise.Custos.Itens);
        Assert.IsNotEmpty(analise.Custos.Produtos);
        Assert.AreEqual(100, analise.Entradas.QuantidadeAnimais);
        Assert.AreEqual(340m, analise.Premissas.Financeiro.PrecoVendaArrobaVivaReais);
        Assert.IsNotEmpty(analise.Formulas);
        using var documento = JsonDocument.Parse(JsonSerializer.Serialize(relatorio));
        var propriedades = Propriedades(documento.RootElement);
        foreach (var nome in new[] { "Entradas", "Componentes", "GmdKgDia", "Produtos", "Premissas", "Formulas" })
            Assert.Contains(nome, propriedades);
    }

    [TestMethod]
    public async Task HistoricoRespeitaPerfilVigenteSemModificarResultadoOuRecalcular()
    {
        var cenario = new CenarioRebanhos();
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var primeiro = (await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId))).Value;
        SemDetalhesRestritos(JsonSerializer.Serialize(primeiro));
        var score = ((ResumoPecuaristaDto)primeiro.Relatorio).Score;
        var lucro = ((ResumoPecuaristaDto)primeiro.Relatorio).Financeiro.LucroReais;
        cenario.Usuario.Conta = new("autor-a", PerfisNegocio.Cooperativa);
        var ampliado = (await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId))).Value;
        Assert.IsInstanceOfType<RelatorioCooperativaDto>(ampliado.Relatorio);
        Assert.AreEqual(score, ((RelatorioCooperativaDto)ampliado.Relatorio).Analise.Score.Total);
        Assert.AreEqual(lucro, ((RelatorioCooperativaDto)ampliado.Relatorio).Analise.Financeiro.Referencia.LucroReais);
        cenario.Usuario.Conta = new("autor-a", PerfisNegocio.Pecuarista);
        var reduzido = (await cenario.Historico.ExecutarAsync(new(criado.UltimoScoreId))).Value;
        SemDetalhesRestritos(JsonSerializer.Serialize(reduzido));
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
        Assert.AreEqual(1, cenario.Persistencia.Gravacoes);
    }

    [TestMethod]
    public async Task MemoriaDoHistoricoUsaTextoSalvoSemSubstituirPeloCatalogoAtual()
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", PerfisNegocio.Cooperativa);
        var analise = cenario.Calcular.Executar(CenarioRebanhos.Entrada.Entradas).Value with
        {
            Formulas = Array.AsReadOnly(new[] { new FormulaAnaliseDto("Memória original", "Expressão preservada no cálculo original.") })
        };
        var id = await cenario.Persistencia.SalvarAvulsoAsync("autor-a", analise);
        var relatorio = (await cenario.Historico.ExecutarAsync(new(id))).Value;
        var salvo = ((RelatorioCooperativaDto)relatorio.Relatorio).Analise;
        Assert.HasCount(1, salvo.Formulas);
        Assert.AreEqual("Memória original", salvo.Formulas[0].Indicador);
        Assert.AreEqual("Expressão preservada no cálculo original.", salvo.Formulas[0].Expressao);
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
    }

    [TestMethod]
    public async Task PrejuizoSuperlotacaoEAlertasSanitariosContinuamNoResumo()
    {
        var cenario = new CenarioRebanhos();
        var entrada = CenarioRebanhos.Entrada with
        {
            Entradas = CenarioRebanhos.Entrada.Entradas with
            {
                QuantidadeAnimais = 300, PrecoCompraPorKgReais = 30,
                VacinaCarbunculo = false, ControleCarrapato = false, Vermifugo = false
            }
        };
        var criado = (await cenario.Criar.ExecutarAsync(entrada)).Value;
        var resumo = (ResumoPecuaristaDto)(await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value.Relatorio;
        Assert.IsLessThan(0m, resumo.Financeiro.LucroReais);
        foreach (var codigo in new[] { "PREJUIZO", "SUPERLOTACAO", "SEM_VACINA_CARBUNCULO", "SEM_CONTROLE_CARRAPATO", "SEM_VERMIFUGO" })
            Assert.IsTrue(resumo.Alertas.Any(a => a.Codigo == codigo));
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task EditarERecalcularMantemNivelDeVisualizacao(string perfil)
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", perfil);
        var criado = (await cenario.Criar.ExecutarAsync(CenarioRebanhos.Entrada)).Value;
        var atualizado = (await cenario.Atualizar.ExecutarAsync(new(criado.Id, criado.Versao, "Lote", "Fazenda",
            CenarioRebanhos.Entrada.Entradas with { DiasPastagem = 300 }))).Value;
        var relatorio = (await cenario.Consultar.ExecutarAsync(new(criado.Id))).Value;
        Assert.AreEqual(atualizado.UltimoScoreId, relatorio.UltimoScoreId);
        if (perfil == PerfisNegocio.Pecuarista)
            SemDetalhesRestritos(JsonSerializer.Serialize(relatorio));
        else
            Assert.IsInstanceOfType<RelatorioCooperativaDto>(relatorio.Relatorio);
    }

    [TestMethod]
    public async Task SnapshotAnteriorSemMemoriaNaoRecebeFormulasDoCodigoAtual()
    {
        var cenario = new CenarioRebanhos();
        cenario.Usuario.Conta = new("autor-a", PerfisNegocio.Cooperativa);
        var analise = cenario.Calcular.Executar(CenarioRebanhos.Entrada.Entradas).Value;
        var json = System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(analise))!;
        json.AsObject().Remove("Formulas");
        var legado = json.Deserialize<AnaliseCompletaDto>()!;
        var id = await cenario.Persistencia.SalvarAvulsoAsync("autor-a", legado);
        var relatorio = (await cenario.Historico.ExecutarAsync(new(id))).Value;
        Assert.IsEmpty(((RelatorioCooperativaDto)relatorio.Relatorio).Analise.Formulas);
        Assert.AreEqual(analise.Score.Total, ((RelatorioCooperativaDto)relatorio.Relatorio).Analise.Score.Total);
    }

    private static void SemDetalhesRestritos(string json)
    {
        using var documento = JsonDocument.Parse(json);
        var propriedades = Propriedades(documento.RootElement);
        foreach (var nome in new[]
        {
            "Entradas", "Componentes", "GmdKgDia", "MortalidadeDecimal", "AnimaisVendidosEstimados",
            "PesoTotalVendidoKg", "ArrobasVivasVendidas", "Produtos", "Itens", "Premissas", "Formulas",
            "Aplicacoes", "DosePorAnimal", "QuantidadeEmbalagens", "PrecoArrobaVivaReais"
        })
            Assert.DoesNotContain(nome, propriedades, $"Campo restrito encontrado: {nome}");
    }

    private static HashSet<string> Propriedades(JsonElement elemento)
    {
        var nomes = new HashSet<string>(StringComparer.Ordinal);
        if (elemento.ValueKind == JsonValueKind.Object)
            foreach (var propriedade in elemento.EnumerateObject())
            {
                nomes.Add(propriedade.Name);
                nomes.UnionWith(Propriedades(propriedade.Value));
            }
        if (elemento.ValueKind == JsonValueKind.Array)
            foreach (var item in elemento.EnumerateArray())
                nomes.UnionWith(Propriedades(item));
        return nomes;
    }
}
