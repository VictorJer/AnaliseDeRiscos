namespace ScoreRebanho.Domain.Custos;

public sealed record ConsumoProduto(string Produto, decimal DosePorAnimal, string UnidadeDose,
    decimal Aplicacoes, decimal QuantidadeNecessaria, decimal QuantidadeEmbalagens,
    decimal CustoReais);
