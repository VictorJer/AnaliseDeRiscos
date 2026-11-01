namespace ScoreRebanho.Domain.Custos;

public sealed class ResultadoCustos
{
    public string VersaoPremissas { get; }
    public IReadOnlyList<ItemCusto> Itens { get; }
    public IReadOnlyList<ConsumoProduto> Produtos { get; }
    public decimal CustoTotalReais { get; }
    public decimal CapitalInicialReais { get; }

    internal ResultadoCustos(string versaoPremissas, IEnumerable<ItemCusto> itens, IEnumerable<ConsumoProduto> produtos)
    {
        VersaoPremissas = versaoPremissas;
        Itens = Array.AsReadOnly(itens.ToArray());
        Produtos = Array.AsReadOnly(produtos.ToArray());
        CustoTotalReais = Itens.Sum(i => i.CustoOperacaoReais);
        CapitalInicialReais = Itens.Sum(i => i.CapitalInicialReais);
    }
}
