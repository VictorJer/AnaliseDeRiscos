namespace ScoreRebanho.Domain.Custos;

public sealed record FormacaoPasto(decimal SementeReaisPorHectare, decimal CalcarioReaisPorHectare,
    decimal AduboBaseReaisPorHectare, decimal UreiaReaisPorHectare, decimal PreparoReaisPorHectare)
{
    public decimal TotalReaisPorHectare => SementeReaisPorHectare + CalcarioReaisPorHectare
        + AduboBaseReaisPorHectare + UreiaReaisPorHectare + PreparoReaisPorHectare;

    public void Validar()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(SementeReaisPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegative(CalcarioReaisPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegative(AduboBaseReaisPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegative(UreiaReaisPorHectare);
        ArgumentOutOfRangeException.ThrowIfNegative(PreparoReaisPorHectare);
        _ = TotalReaisPorHectare;
    }
}
