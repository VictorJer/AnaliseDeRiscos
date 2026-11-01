namespace ScoreRebanho.Domain.Financeiro;

public sealed record CenarioFinanceiro(decimal PrecoArrobaVivaReais, decimal ReceitaReais,
    decimal LucroReais, decimal? MargemDecimal);
