using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Relatorios;

internal static class ProjecaoRelatorio
{
    internal static RelatorioAnaliseDto Criar(AnaliseCompletaDto analise, ContaAutenticada conta) =>
        conta.PerfilNegocio switch
        {
            PerfisNegocio.Pecuarista => Resumo(analise),
            PerfisNegocio.Cooperativa => new RelatorioCooperativaDto(analise),
            _ => throw new InvalidOperationException("A projeção requer um perfil de negócio autorizado.")
        };

    private static ResumoPecuaristaDto Resumo(AnaliseCompletaDto analise)
    {
        var referencia = analise.Financeiro.Referencia;
        return new(analise.CalculadaEmUtc, analise.Premissas.Versao, analise.Score.Total,
            analise.Score.Classificacao, analise.ExplicacaoResumida, analise.Alertas,
            new(analise.Lotacao.OcupacaoDecimal, analise.Lotacao.CapacidadeUa, analise.Lotacao.QuantidadeMaximaAnimais),
            analise.Producao.PesoVendaPorAnimalKg,
            new(analise.Custos.CustoTotalReais, analise.Custos.CapitalInicialReais, referencia.ReceitaReais,
                referencia.LucroReais, referencia.MargemDecimal, analise.Financeiro.PrecoEquilibrioPorKgReais,
                analise.Financeiro.MotivoEquilibrioIndisponivel
                    ?? "É o preço de venda estimado por kg vivo necessário para cobrir os custos da operação."),
            analise.Financeiro.Adverso.LucroReais,
            Array.AsReadOnly(new[]
            {
                "Os resultados são estimativas; confirme os preços e as condições na sua região.",
                "O score não garante lucro, segurança da operação ou aprovação de crédito.",
                "O capital inicial não inclui manutenção do pasto, arrendamento ou suplementação. Planeje também essas despesas."
            }));
    }
}
