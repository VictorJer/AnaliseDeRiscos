using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Analises;

public sealed class AvaliadorRiscos(PremissasAnalise premissas)
{
    private readonly PremissasAnalise premissas = premissas ?? throw new ArgumentNullException(nameof(premissas));

    public IReadOnlyList<AlertaAnalise> Avaliar(DadosSimulacao dados, ResultadoLotacao lotacao, ResultadoFinanceiro financeiro)
    {
        ArgumentNullException.ThrowIfNull(dados);
        ArgumentNullException.ThrowIfNull(lotacao);
        ArgumentNullException.ThrowIfNull(financeiro);
        var alertas = new List<AlertaAnalise>();
        if (financeiro.Referencia.LucroReais < 0m)
            alertas.Add(new("PREJUIZO", "As receitas estimadas não cobrem os custos da operação.",
                "Revise o preço de compra, os custos e o preço esperado de venda antes de investir."));
        if (lotacao.OcupacaoDecimal > premissas.Score.OcupacaoMaximaIdeal)
            alertas.Add(new("SUPERLOTACAO", "A quantidade de animais excede a capacidade estimada do pasto.",
                "Avalie reduzir o lote ou ampliar a área disponível; confira a capacidade estimada de animais."));
        if (!dados.Manejo.VacinaCarbunculo)
            alertas.Add(new("SEM_VACINA_CARBUNCULO", "A vacinação contra carbúnculo não foi informada.",
                "Revise o planejamento sanitário com orientação veterinária."));
        if (!dados.Manejo.ControleCarrapato)
            alertas.Add(new("SEM_CONTROLE_CARRAPATO", "O controle de carrapato não foi informado.",
                "Revise o controle de carrapato com orientação veterinária."));
        if (!dados.Manejo.Vermifugo)
            alertas.Add(new("SEM_VERMIFUGO", "O uso de vermífugo não foi informado.",
                "Revise o controle de verminoses com orientação veterinária."));
        if (financeiro.Referencia.LucroReais >= 0m && financeiro.Referencia.MargemDecimal < premissas.Score.MargemPontosMaximos)
            alertas.Add(new("MARGEM_BAIXA", "A margem estimada está abaixo da referência para a pontuação financeira máxima.",
                "Confira os preços e as despesas; há menor folga para absorver imprevistos."));
        if (financeiro.Referencia.LucroReais >= 0m && financeiro.Adverso.LucroReais < 0m)
            alertas.Add(new("PREJUIZO_CENARIO_ADVERSO", "Uma queda no preço de venda pode levar a operação ao prejuízo.",
                "Considere o cenário adverso ao planejar a necessidade de recursos."));
        if (lotacao.OcupacaoDecimal < premissas.Score.OcupacaoMinimaIdeal)
            alertas.Add(new("SUBOCUPACAO", "A ocupação está abaixo da faixa de referência do score.",
                "Avalie o aproveitamento da área sem ultrapassar a capacidade estimada do pasto."));
        if (dados.Lotacao.RotacaoAtiva && dados.Lotacao.NumeroPiquetes < premissas.Lotacao.MinimoPiquetesComBeneficio)
            alertas.Add(new("ROTACAO_SEM_BENEFICIO", "A rotação informada gera custo, mas os piquetes são insuficientes para o benefício estimado.",
                "Revise o número de piquetes e o investimento antes de implantar a rotação."));
        return alertas.AsReadOnly();
    }
}
