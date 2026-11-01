using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Application.Simulacoes;

public sealed record EntradaSimulacaoDto
{
    public decimal AreaHectares { get; init; }
    public TipoPastagem Pastagem { get; init; }
    public TipoPosse Posse { get; init; }
    public decimal? ArrendamentoMensalReais { get; init; }
    public bool PastoPronto { get; init; }
    public bool RotacaoAtiva { get; init; }
    public int? NumeroPiquetes { get; init; }
    public int QuantidadeAnimais { get; init; }
    public decimal PesoCompraKg { get; init; }
    public decimal PrecoCompraPorKgReais { get; init; }
    public int DiasPastagem { get; init; }
    public bool RacaPuraCorte { get; init; }
    public bool VacinaCarbunculo { get; init; }
    public bool ControleCarrapato { get; init; }
    public bool Vermifugo { get; init; }
    public bool ModificadorOrganico { get; init; }
}
