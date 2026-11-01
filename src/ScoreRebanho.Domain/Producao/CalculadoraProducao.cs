using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Producao;

public sealed class CalculadoraProducao
{
    private readonly CalculadoraLotacao lotacao;
    private readonly PremissasProducao premissas;

    public CalculadoraProducao(PremissasLotacao premissasLotacao, PremissasProducao premissas)
    {
        this.premissas = premissas ?? throw new ArgumentNullException(nameof(premissas));
        ArgumentNullException.ThrowIfNull(premissasLotacao);
        if (premissasLotacao.Versao != premissas.Versao)
            throw new ArgumentException("As premissas de lotação e produção devem ter a mesma versão.", nameof(premissas));
        lotacao = new CalculadoraLotacao(premissasLotacao);
    }

    public ResultadoProducao Calcular(DadosSimulacao dados)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var capacidade = lotacao.Calcular(dados.Lotacao);
        var excedente = Math.Max(0m, capacidade.OcupacaoDecimal - 1m);
        var fator = Math.Max(premissas.FatorMinimoGmd, 1m - premissas.PenalidadeGmdPorExcedente * excedente);
        var manejo = dados.Manejo;
        var gmd = (premissas.GmdBaseKgDia
            + (dados.RacaPuraCorte ? premissas.BonusRacaKgDia : 0m)
            + (manejo.ControleCarrapato ? premissas.BonusCarrapatoKgDia : 0m)
            + (manejo.Vermifugo ? premissas.BonusVermifugoKgDia : 0m)
            + (manejo.ModificadorOrganico ? premissas.BonusModificadorKgDia : 0m)) * fator;
        var mortalidade = Math.Min(premissas.MortalidadeMaxima,
            premissas.MortalidadeBase
            + (manejo.VacinaCarbunculo ? 0m : premissas.MortalidadeSemVacina)
            + (manejo.ControleCarrapato ? 0m : premissas.MortalidadeSemCarrapato)
            + (manejo.Vermifugo ? 0m : premissas.MortalidadeSemVermifugo)
            + premissas.MortalidadePorExcedente * excedente);
        var pesoVenda = dados.Lotacao.PesoCompraKg + gmd * dados.DiasPastagem;
        var animaisVendidos = dados.Lotacao.QuantidadeAnimais * (1m - mortalidade);
        var pesoTotal = animaisVendidos * pesoVenda;
        return new(capacidade, fator, gmd, pesoVenda, mortalidade, animaisVendidos,
            pesoTotal, pesoTotal / premissas.QuilogramasPorArrobaViva);
    }
}
