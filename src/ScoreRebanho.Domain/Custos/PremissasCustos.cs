namespace ScoreRebanho.Domain.Custos;

public sealed record PremissasCustos
{
    public string Versao { get; init; } = "1.0";
    public int DiasAno { get; init; } = 365;
    public int DiasMesArrendamento { get; init; } = 30;
    public int AnosAmortizacaoRotacao { get; init; } = 10;
    public decimal ManutencaoVeraoReaisHaAno { get; init; } = 300m;
    public decimal ManutencaoInvernoReaisHaAno { get; init; } = 450m;
    public FormacaoPasto FormacaoVerao { get; init; } = new(420, 375, 640, 350, 600);
    public FormacaoPasto FormacaoInverno { get; init; } = new(425, 250, 800, 525, 400);
    public decimal FracaoEconomicaFormacaoVerao { get; init; } = 0.2m;
    public decimal RotacaoBaseReaisHa { get; init; } = 150m;
    public decimal RotacaoReaisHaPorPiquete { get; init; } = 60m;
    public decimal InicioSuplementacaoOcupacao { get; init; } = 1.10m;
    public decimal SuplementacaoReaisAnimalDia { get; init; } = 3m;
    public ProdutoSanitario Carbunculo { get; init; } = new(2, 100, 40, null);
    public ProdutoSanitario Carrapato { get; init; } = new(4, 500, 100, 90);
    public ProdutoSanitario Vermifugo { get; init; } = new(6, 500, 85, 150);
    public decimal ModificadorKgAnimalDia { get; init; } = 0.05m;
    public decimal ModificadorKgPorSaco { get; init; } = 25m;
    public decimal ModificadorPrecoSacoReais { get; init; } = 120m;

    public static PremissasCustos Versao1 { get; } = new();

    public void Validar()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Versao);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DiasAno);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DiasMesArrendamento);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(AnosAmortizacaoRotacao);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ModificadorKgPorSaco);
        ArgumentOutOfRangeException.ThrowIfNegative(ManutencaoVeraoReaisHaAno);
        ArgumentOutOfRangeException.ThrowIfNegative(ManutencaoInvernoReaisHaAno);
        ArgumentOutOfRangeException.ThrowIfNegative(RotacaoBaseReaisHa);
        ArgumentOutOfRangeException.ThrowIfNegative(RotacaoReaisHaPorPiquete);
        ArgumentOutOfRangeException.ThrowIfNegative(InicioSuplementacaoOcupacao);
        ArgumentOutOfRangeException.ThrowIfNegative(SuplementacaoReaisAnimalDia);
        ArgumentOutOfRangeException.ThrowIfNegative(ModificadorKgAnimalDia);
        ArgumentOutOfRangeException.ThrowIfNegative(ModificadorPrecoSacoReais);
        ArgumentOutOfRangeException.ThrowIfNegative(FracaoEconomicaFormacaoVerao);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(FracaoEconomicaFormacaoVerao, 1m);
        ArgumentNullException.ThrowIfNull(FormacaoVerao);
        ArgumentNullException.ThrowIfNull(FormacaoInverno);
        ArgumentNullException.ThrowIfNull(Carbunculo);
        ArgumentNullException.ThrowIfNull(Carrapato);
        ArgumentNullException.ThrowIfNull(Vermifugo);
        FormacaoVerao.Validar();
        FormacaoInverno.Validar();
        Carbunculo.Validar();
        Carrapato.Validar();
        Vermifugo.Validar();
    }
}
