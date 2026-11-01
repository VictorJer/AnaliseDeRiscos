namespace ScoreRebanho.Domain.Scores;

public sealed record PremissasScore
{
    public string Versao { get; init; } = "1.0";
    public decimal MaximoLotacao { get; init; } = 30m;
    public decimal OcupacaoMinimaIdeal { get; init; } = 0.5m;
    public decimal OcupacaoMaximaIdeal { get; init; } = 1m;
    public decimal OcupacaoSemPontos { get; init; } = 1.8m;
    public decimal PerdaMaximaSubocupacao { get; init; } = 12m;
    public decimal PontosVacina { get; init; } = 8m;
    public decimal PontosCarrapato { get; init; } = 7m;
    public decimal PontosVermifugo { get; init; } = 7m;
    public decimal PontosModificador { get; init; } = 3m;
    public decimal MaximoFinanceiro { get; init; } = 30m;
    public decimal MargemSemPontos { get; init; } = -0.10m;
    public decimal MargemPontosMaximos { get; init; } = 0.20m;
    public decimal PontosRaca { get; init; } = 6m;
    public int DiasMinimosIdeais { get; init; } = 120;
    public int DiasMaximosIdeais { get; init; } = 365;
    public decimal PontosPrazoIdeal { get; init; } = 3m;
    public decimal PontosPrazoForaIdeal { get; init; } = 1m;
    public decimal PontosPossePropria { get; init; } = 2m;
    public decimal PontosRotacao { get; init; } = 4m;

    public static PremissasScore Versao1 { get; } = new();

    public void Validar()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Versao);
        foreach (var valor in new[] { MaximoLotacao, PerdaMaximaSubocupacao, PontosVacina, PontosCarrapato,
            PontosVermifugo, PontosModificador, MaximoFinanceiro, PontosRaca, PontosPrazoIdeal,
            PontosPrazoForaIdeal, PontosPossePropria, PontosRotacao })
        {
            ArgumentOutOfRangeException.ThrowIfNegative(valor);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(valor, ResultadoScore.Maximo);
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(OcupacaoMinimaIdeal);
        ArgumentOutOfRangeException.ThrowIfLessThan(OcupacaoMaximaIdeal, OcupacaoMinimaIdeal);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(OcupacaoSemPontos, OcupacaoMaximaIdeal);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(PerdaMaximaSubocupacao, MaximoLotacao);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MargemPontosMaximos, MargemSemPontos);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DiasMinimosIdeais);
        ArgumentOutOfRangeException.ThrowIfLessThan(DiasMaximosIdeais, DiasMinimosIdeais);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(PontosPrazoForaIdeal, PontosPrazoIdeal);
    }
}
