using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Producao;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Simulacoes;

internal static class ProjecaoAnalise
{
    internal static AnaliseCompletaDto Completa(EntradaSimulacaoDto entrada, PremissasAnalise premissas,
        ResultadoProducao producao, ResultadoCustos custos, ResultadoFinanceiro financeiro,
        ResultadoScore score, IReadOnlyList<AlertaAnalise> alertas, DateTimeOffset calculadaEmUtc)
    {
        var lotacao = producao.Lotacao;
        return new(calculadaEmUtc, entrada,
            new(premissas.Versao, premissas.Lotacao, premissas.Producao, premissas.Custos,
                premissas.Financeiro, premissas.Score, ResultadoScore.Maximo, ResultadoScore.LimiteRiscoBaixo,
                ResultadoScore.LimiteRiscoModerado, ResultadoScore.LimiteRiscoAlto,
                "Inteiro, empate para cima, antes da classificação.",
                "Uma aplicação inicial mais piso(dias/intervalo), incluindo último dia coincidente.",
                "Frascos sobre volume total e sacos sobre consumo total; arredondar para cima."),
            new(lotacao.UaLote, lotacao.MultiplicadorRotacao, lotacao.CapacidadeSemRotacaoUa, lotacao.CapacidadeUa,
                lotacao.OcupacaoDecimal, lotacao.QuantidadeMaximaAnimaisSemRotacao, lotacao.QuantidadeMaximaAnimais),
            new(producao.FatorGmd, producao.GmdKgDia, producao.PesoVendaPorAnimalKg, producao.MortalidadeDecimal,
                producao.AnimaisVendidosEstimados, producao.PesoTotalVendidoKg, producao.ArrobasVivasVendidas),
            new(custos.CustoTotalReais, custos.CapitalInicialReais,
                SomenteLeitura(custos.Itens.Select(i => new ItemCustoDto(i.Descricao, i.CustoOperacaoReais, i.CapitalInicialReais))),
                SomenteLeitura(custos.Produtos.Select(p => new ConsumoProdutoDto(p.Produto, p.DosePorAnimal, p.UnidadeDose,
                    p.Aplicacoes, p.QuantidadeNecessaria, p.QuantidadeEmbalagens, p.CustoReais)))),
            new(financeiro.PrecoEquilibrioPorKgReais, financeiro.MotivoEquilibrioIndisponivel, financeiro.MotivoMargemIndisponivel,
                Cenario(financeiro.Referencia), Cenario(financeiro.Adverso)),
            new(score.Total, score.Classificacao,
                SomenteLeitura(score.Componentes.Select(c => new ComponenteScoreDto(c.Nome, c.Pontos, c.Justificativa)))),
            SomenteLeitura(alertas.Select(a => new AlertaAnaliseDto(a.Codigo, a.Mensagem, a.Recomendacao))),
            Explicacao(score.Classificacao),
            Array.AsReadOnly(new[]
            {
                "Os resultados são estimativas; confirme os valores de referência na região.",
                "O score não garante lucro, segurança da operação ou aprovação de crédito.",
                "O capital inicial não inclui manutenção do pasto, arrendamento ou suplementação; planeje essas despesas durante a operação.",
                "Medicamentos e modificador usam a quantidade inicial de animais; embalagens são arredondadas para cima.",
                "O cenário adverso altera somente o preço de venda e os indicadores financeiros, preservando produção e custos."
            }));
    }

    private static IReadOnlyList<T> SomenteLeitura<T>(IEnumerable<T> valores) => Array.AsReadOnly(valores.ToArray());

    private static CenarioFinanceiroDto Cenario(CenarioFinanceiro cenario) =>
        new(cenario.PrecoArrobaVivaReais, cenario.ReceitaReais, cenario.LucroReais, cenario.MargemDecimal);

    private static string Explicacao(ClassificacaoRisco risco) => risco switch
    {
        ClassificacaoRisco.Baixo => "A estimativa indica risco baixo nas condições informadas. Confira os alertas e as premissas antes de decidir.",
        ClassificacaoRisco.Moderado => "A estimativa indica risco moderado. Revise os fatores de atenção e a folga financeira da operação.",
        ClassificacaoRisco.Alto => "A estimativa indica risco alto. Revise a lotação, os cuidados sanitários e os custos antes de investir.",
        ClassificacaoRisco.MuitoAlto => "A estimativa indica risco muito alto. Reavalie o planejamento e os principais fatores de risco antes de investir.",
        _ => throw new ArgumentOutOfRangeException(nameof(risco))
    };
}
