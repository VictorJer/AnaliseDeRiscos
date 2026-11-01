using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Scores;

public sealed class CalculadoraScore
{
    private readonly PremissasScore premissas;
    private readonly PremissasLotacao lotacao;

    public CalculadoraScore(PremissasLotacao lotacao, PremissasScore premissas)
    {
        ArgumentNullException.ThrowIfNull(lotacao);
        ArgumentNullException.ThrowIfNull(premissas);
        premissas.Validar();
        if (lotacao.Versao != premissas.Versao)
            throw new ArgumentException("As premissas de score e lotação devem ter a mesma versão.", nameof(premissas));
        this.premissas = premissas;
        this.lotacao = lotacao;
    }

    public ResultadoScore Calcular(DadosSimulacao dados, decimal ocupacaoDecimal, decimal margemDecimal)
    {
        ArgumentNullException.ThrowIfNull(dados);
        ArgumentOutOfRangeException.ThrowIfNegative(ocupacaoDecimal);
        var manejo = dados.Manejo;
        var pontosSanidade = (manejo.VacinaCarbunculo ? premissas.PontosVacina : 0m)
            + (manejo.ControleCarrapato ? premissas.PontosCarrapato : 0m)
            + (manejo.Vermifugo ? premissas.PontosVermifugo : 0m)
            + (manejo.ModificadorOrganico ? premissas.PontosModificador : 0m);
        var pontosPrazo = dados.DiasPastagem >= premissas.DiasMinimosIdeais && dados.DiasPastagem <= premissas.DiasMaximosIdeais
            ? premissas.PontosPrazoIdeal : premissas.PontosPrazoForaIdeal;
        var rotacaoEfetiva = dados.Lotacao.RotacaoAtiva && dados.Lotacao.NumeroPiquetes >= lotacao.MinimoPiquetesComBeneficio;
        var pontosManejo = (dados.RacaPuraCorte ? premissas.PontosRaca : 0m)
            + pontosPrazo + (dados.Posse == TipoPosse.Proprio ? premissas.PontosPossePropria : 0m)
            + (rotacaoEfetiva ? premissas.PontosRotacao : 0m);
        return new(premissas.Versao,
        [
            new("Lotação", PontosLotacao(ocupacaoDecimal),
                ocupacaoDecimal < premissas.OcupacaoMinimaIdeal ? "Ocupação abaixo da faixa ideal."
                : ocupacaoDecimal > premissas.OcupacaoMaximaIdeal ? "Ocupação acima da capacidade do pasto." : "Ocupação na faixa ideal."),
            new("Sanidade", pontosSanidade, "Pontos pelos cuidados sanitários e pelo modificador orgânico informados."),
            new("Resultado financeiro", PontosFinanceiros(margemDecimal), "Pontuação proporcional à margem estimada sobre o custo da operação."),
            new("Manejo e custos fixos", pontosManejo, "Considera raça, prazo na pastagem, posse do terreno e rotação com piquetes suficientes.")
        ]);
    }

    private decimal PontosLotacao(decimal ocupacao)
    {
        if (ocupacao < premissas.OcupacaoMinimaIdeal)
            return premissas.MaximoLotacao - premissas.PerdaMaximaSubocupacao * (1m - ocupacao / premissas.OcupacaoMinimaIdeal);
        if (ocupacao <= premissas.OcupacaoMaximaIdeal)
            return premissas.MaximoLotacao;
        if (ocupacao >= premissas.OcupacaoSemPontos)
            return 0m;
        return premissas.MaximoLotacao * (premissas.OcupacaoSemPontos - ocupacao)
            / (premissas.OcupacaoSemPontos - premissas.OcupacaoMaximaIdeal);
    }

    private decimal PontosFinanceiros(decimal margem)
    {
        if (margem <= premissas.MargemSemPontos)
            return 0m;
        if (margem >= premissas.MargemPontosMaximos)
            return premissas.MaximoFinanceiro;
        return premissas.MaximoFinanceiro * (margem - premissas.MargemSemPontos)
            / (premissas.MargemPontosMaximos - premissas.MargemSemPontos);
    }
}
