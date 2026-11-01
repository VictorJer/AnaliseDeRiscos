using ScoreRebanho.Domain.Pastagens;

namespace ScoreRebanho.Domain.Simulacoes;

public sealed record DadosSimulacao
{
    public DadosLotacao Lotacao { get; }
    public TipoPosse Posse { get; }
    public decimal ArrendamentoMensalReais { get; }
    public bool PastoPronto { get; }
    public decimal PrecoCompraPorKgReais { get; }
    public int DiasPastagem { get; }
    public bool RacaPuraCorte { get; }
    public ManejoSanitario Manejo { get; }

    public DadosSimulacao(DadosLotacao lotacao, TipoPosse posse, decimal? arrendamentoMensalReais,
        bool pastoPronto, decimal precoCompraPorKgReais, int diasPastagem,
        bool racaPuraCorte, ManejoSanitario manejo)
    {
        ArgumentNullException.ThrowIfNull(lotacao);
        ArgumentNullException.ThrowIfNull(manejo);
        if (!Enum.IsDefined(posse))
            throw new ArgumentOutOfRangeException(nameof(posse), "Informe um tipo de posse válido.");
        if (posse == TipoPosse.Arrendado && arrendamentoMensalReais is null)
            throw new ArgumentException("Informe o valor mensal do arrendamento.", nameof(arrendamentoMensalReais));
        if (arrendamentoMensalReais < 0)
            throw new ArgumentOutOfRangeException(nameof(arrendamentoMensalReais), "O arrendamento não pode ser negativo.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(precoCompraPorKgReais);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diasPastagem);

        Lotacao = lotacao;
        Posse = posse;
        ArrendamentoMensalReais = posse == TipoPosse.Arrendado ? arrendamentoMensalReais!.Value : 0m;
        PastoPronto = pastoPronto;
        PrecoCompraPorKgReais = precoCompraPorKgReais;
        DiasPastagem = diasPastagem;
        RacaPuraCorte = racaPuraCorte;
        Manejo = manejo;
    }
}
