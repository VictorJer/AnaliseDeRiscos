using ScoreRebanho.Domain.Producao;

namespace ScoreRebanho.Domain.Financeiro;

public sealed class CalculadoraFinanceira
{
    private readonly PremissasFinanceiras premissas;
    private readonly decimal quilogramasPorArroba;

    public CalculadoraFinanceira(PremissasProducao producao, PremissasFinanceiras premissas)
    {
        ArgumentNullException.ThrowIfNull(producao);
        ArgumentNullException.ThrowIfNull(premissas);
        premissas.Validar();
        if (producao.Versao != premissas.Versao)
            throw new ArgumentException("As premissas financeiras e de produção devem ter a mesma versão.", nameof(premissas));
        this.premissas = premissas;
        quilogramasPorArroba = producao.QuilogramasPorArrobaViva;
    }

    public ResultadoFinanceiro Calcular(decimal custoTotalReais, decimal pesoTotalVendidoKg)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(custoTotalReais);
        ArgumentOutOfRangeException.ThrowIfNegative(pesoTotalVendidoKg);
        var arrobas = pesoTotalVendidoKg / quilogramasPorArroba;
        var referencia = CalcularCenario(premissas.PrecoVendaArrobaVivaReais, arrobas, custoTotalReais);
        var adverso = CalcularCenario(premissas.PrecoVendaArrobaVivaReais * (1m - premissas.QuedaPrecoCenarioAdverso), arrobas, custoTotalReais);
        return new(premissas.Versao, custoTotalReais, pesoTotalVendidoKg, arrobas,
            pesoTotalVendidoKg > 0m ? custoTotalReais / pesoTotalVendidoKg : null,
            pesoTotalVendidoKg == 0m ? "Não há peso vendido para calcular o preço de equilíbrio." : null,
            custoTotalReais == 0m ? "O custo total é zero; a margem sobre o custo não está disponível." : null,
            referencia, adverso);
    }

    private static CenarioFinanceiro CalcularCenario(decimal preco, decimal arrobas, decimal custo)
    {
        var receita = arrobas * preco;
        var lucro = receita - custo;
        return new(preco, receita, lucro, custo > 0m ? lucro / custo : null);
    }
}
