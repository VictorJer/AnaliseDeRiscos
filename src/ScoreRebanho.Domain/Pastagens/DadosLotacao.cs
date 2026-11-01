namespace ScoreRebanho.Domain.Pastagens;

public sealed record DadosLotacao
{
    public decimal AreaHectares { get; }
    public TipoPastagem Pastagem { get; }
    public int QuantidadeAnimais { get; }
    public decimal PesoCompraKg { get; }
    public bool RotacaoAtiva { get; }
    public int? NumeroPiquetes { get; }

    public DadosLotacao(decimal areaHectares, TipoPastagem pastagem, int quantidadeAnimais,
        decimal pesoCompraKg, bool rotacaoAtiva, int? numeroPiquetes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(areaHectares);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidadeAnimais);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pesoCompraKg);
        if (!Enum.IsDefined(pastagem))
            throw new ArgumentOutOfRangeException(nameof(pastagem), "Informe uma pastagem válida.");
        if (rotacaoAtiva && (!numeroPiquetes.HasValue || numeroPiquetes.Value <= 0))
            throw new ArgumentException("Informe uma quantidade positiva de piquetes.", nameof(numeroPiquetes));
        if (numeroPiquetes < 0)
            throw new ArgumentOutOfRangeException(nameof(numeroPiquetes));
        AreaHectares = areaHectares;
        Pastagem = pastagem;
        QuantidadeAnimais = quantidadeAnimais;
        PesoCompraKg = pesoCompraKg;
        RotacaoAtiva = rotacaoAtiva;
        NumeroPiquetes = numeroPiquetes;
    }
}
