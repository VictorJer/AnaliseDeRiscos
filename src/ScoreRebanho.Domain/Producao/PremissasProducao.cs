namespace ScoreRebanho.Domain.Producao;

public sealed record PremissasProducao
{
    public string Versao { get; }
    public decimal GmdBaseKgDia { get; }
    public decimal BonusRacaKgDia { get; }
    public decimal BonusCarrapatoKgDia { get; }
    public decimal BonusVermifugoKgDia { get; }
    public decimal BonusModificadorKgDia { get; }
    public decimal FatorMinimoGmd { get; }
    public decimal PenalidadeGmdPorExcedente { get; }
    public decimal MortalidadeBase { get; }
    public decimal MortalidadeSemVacina { get; }
    public decimal MortalidadeSemCarrapato { get; }
    public decimal MortalidadeSemVermifugo { get; }
    public decimal MortalidadePorExcedente { get; }
    public decimal MortalidadeMaxima { get; }
    public decimal QuilogramasPorArrobaViva { get; }

    public static PremissasProducao Versao1 { get; } = new(
        "1.0", 0.55m, 0.10m, 0.04m, 0.05m, 0.12m, 0.45m, 0.5m,
        0.015m, 0.015m, 0.012m, 0.008m, 0.04m, 0.15m, 30m);

    public PremissasProducao(string versao, decimal gmdBaseKgDia, decimal bonusRacaKgDia,
        decimal bonusCarrapatoKgDia, decimal bonusVermifugoKgDia, decimal bonusModificadorKgDia,
        decimal fatorMinimoGmd, decimal penalidadeGmdPorExcedente, decimal mortalidadeBase,
        decimal mortalidadeSemVacina, decimal mortalidadeSemCarrapato, decimal mortalidadeSemVermifugo,
        decimal mortalidadePorExcedente, decimal mortalidadeMaxima, decimal quilogramasPorArrobaViva)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versao);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gmdBaseKgDia);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quilogramasPorArrobaViva);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fatorMinimoGmd, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fatorMinimoGmd, 1m);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(mortalidadeMaxima, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mortalidadeMaxima, 1m);
        foreach (var valor in new[] { bonusRacaKgDia, bonusCarrapatoKgDia, bonusVermifugoKgDia,
            bonusModificadorKgDia, penalidadeGmdPorExcedente, mortalidadeBase, mortalidadeSemVacina,
            mortalidadeSemCarrapato, mortalidadeSemVermifugo, mortalidadePorExcedente })
            ArgumentOutOfRangeException.ThrowIfNegative(valor);
        if (mortalidadeBase > mortalidadeMaxima)
            throw new ArgumentException("A mortalidade base não pode exceder o teto.", nameof(mortalidadeBase));

        Versao = versao;
        GmdBaseKgDia = gmdBaseKgDia;
        BonusRacaKgDia = bonusRacaKgDia;
        BonusCarrapatoKgDia = bonusCarrapatoKgDia;
        BonusVermifugoKgDia = bonusVermifugoKgDia;
        BonusModificadorKgDia = bonusModificadorKgDia;
        FatorMinimoGmd = fatorMinimoGmd;
        PenalidadeGmdPorExcedente = penalidadeGmdPorExcedente;
        MortalidadeBase = mortalidadeBase;
        MortalidadeSemVacina = mortalidadeSemVacina;
        MortalidadeSemCarrapato = mortalidadeSemCarrapato;
        MortalidadeSemVermifugo = mortalidadeSemVermifugo;
        MortalidadePorExcedente = mortalidadePorExcedente;
        MortalidadeMaxima = mortalidadeMaxima;
        QuilogramasPorArrobaViva = quilogramasPorArrobaViva;
    }
}
