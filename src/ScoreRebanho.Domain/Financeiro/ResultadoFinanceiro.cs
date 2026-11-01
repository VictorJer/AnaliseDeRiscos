namespace ScoreRebanho.Domain.Financeiro;

public sealed record ResultadoFinanceiro(string VersaoPremissas, decimal CustoTotalReais,
    decimal PesoTotalVendidoKg, decimal ArrobasVivasVendidas, decimal? PrecoEquilibrioPorKgReais,
    string? MotivoEquilibrioIndisponivel, string? MotivoMargemIndisponivel,
    CenarioFinanceiro Referencia, CenarioFinanceiro Adverso);
