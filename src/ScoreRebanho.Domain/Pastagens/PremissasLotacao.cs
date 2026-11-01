namespace ScoreRebanho.Domain.Pastagens;

public sealed record PremissasLotacao
{
    public string Versao { get; }
    public decimal QuilogramasPorUa { get; }
    public decimal CapacidadeVeraoUaPorHectare { get; }
    public decimal CapacidadeInvernoUaPorHectare { get; }
    public decimal IncrementoRotacaoPorPiquete { get; }
    public decimal MultiplicadorMaximoRotacao { get; }
    public int MinimoPiquetesComBeneficio { get; }
    public static PremissasLotacao Versao1 { get; } = new("1.0", 450m, 1m, 1.2m, 0.15m, 1.6m, 3);

    public PremissasLotacao(string versao, decimal quilogramasPorUa,
        decimal capacidadeVeraoUaPorHectare, decimal capacidadeInvernoUaPorHectare,
        decimal incrementoRotacaoPorPiquete, decimal multiplicadorMaximoRotacao,
        int minimoPiquetesComBeneficio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versao);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quilogramasPorUa);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacidadeVeraoUaPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacidadeInvernoUaPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegative(incrementoRotacaoPorPiquete);
        ArgumentOutOfRangeException.ThrowIfLessThan(multiplicadorMaximoRotacao, 1m);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimoPiquetesComBeneficio, 2);
        Versao = versao;
        QuilogramasPorUa = quilogramasPorUa;
        CapacidadeVeraoUaPorHectare = capacidadeVeraoUaPorHectare;
        CapacidadeInvernoUaPorHectare = capacidadeInvernoUaPorHectare;
        IncrementoRotacaoPorPiquete = incrementoRotacaoPorPiquete;
        MultiplicadorMaximoRotacao = multiplicadorMaximoRotacao;
        MinimoPiquetesComBeneficio = minimoPiquetesComBeneficio;
    }
}
