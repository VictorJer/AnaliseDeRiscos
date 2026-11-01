namespace ScoreRebanho.Domain.Pastagens;

public sealed class CalculadoraLotacao(PremissasLotacao premissas)
{
    private readonly PremissasLotacao premissas = premissas ?? throw new ArgumentNullException(nameof(premissas));

    public ResultadoLotacao Calcular(DadosLotacao dados)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var capacidadeBase = dados.Pastagem == TipoPastagem.Verao
            ? premissas.CapacidadeVeraoUaPorHectare : premissas.CapacidadeInvernoUaPorHectare;
        var multiplicador = 1m;
        if (dados.RotacaoAtiva && dados.NumeroPiquetes >= premissas.MinimoPiquetesComBeneficio)
            multiplicador = Math.Min(premissas.MultiplicadorMaximoRotacao,
                1m + premissas.IncrementoRotacaoPorPiquete * (dados.NumeroPiquetes.Value - 1));

        var uaLote = dados.QuantidadeAnimais * dados.PesoCompraKg / premissas.QuilogramasPorUa;
        var capacidadeSemRotacao = dados.AreaHectares * capacidadeBase;
        var capacidade = capacidadeSemRotacao * multiplicador;
        return new ResultadoLotacao(premissas.Versao, uaLote, multiplicador,
            capacidadeSemRotacao, capacidade, uaLote / capacidade,
            decimal.Floor(capacidadeSemRotacao * premissas.QuilogramasPorUa / dados.PesoCompraKg),
            decimal.Floor(capacidade * premissas.QuilogramasPorUa / dados.PesoCompraKg));
    }
}
