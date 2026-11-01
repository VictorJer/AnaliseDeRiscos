namespace ScoreRebanho.Domain.Custos;

public sealed record ProdutoSanitario(decimal DoseMlPorAnimal, decimal VolumeFrascoMl,
    decimal PrecoFrascoReais, int? IntervaloDias)
{
    public void Validar()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DoseMlPorAnimal);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(VolumeFrascoMl);
        ArgumentOutOfRangeException.ThrowIfNegative(PrecoFrascoReais);
        if (IntervaloDias is <= 0)
            throw new ArgumentOutOfRangeException(nameof(IntervaloDias));
    }
}
