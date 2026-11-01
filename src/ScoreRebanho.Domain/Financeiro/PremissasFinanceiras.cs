namespace ScoreRebanho.Domain.Financeiro;

public sealed record PremissasFinanceiras
{
    public string Versao { get; init; } = "1.0";
    public decimal PrecoVendaArrobaVivaReais { get; init; } = 340m;
    public decimal QuedaPrecoCenarioAdverso { get; init; } = 0.10m;
    public static PremissasFinanceiras Versao1 { get; } = new();

    public void Validar()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Versao);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(PrecoVendaArrobaVivaReais);
        ArgumentOutOfRangeException.ThrowIfNegative(QuedaPrecoCenarioAdverso);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(QuedaPrecoCenarioAdverso, 1m);
    }
}
