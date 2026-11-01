namespace ScoreRebanho.Domain.Rebanhos;

public sealed record IdentificacaoRebanho
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoPropriedade = 150;

    public string Nome { get; }
    public string Propriedade { get; }

    public IdentificacaoRebanho(string nome, string propriedade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(propriedade);

        var nomeNormalizado = nome.Trim();
        var propriedadeNormalizada = propriedade.Trim();
        if (nomeNormalizado.Length > TamanhoMaximoNome)
            throw new ArgumentException($"O nome do rebanho deve ter até {TamanhoMaximoNome} caracteres.", nameof(nome));
        if (propriedadeNormalizada.Length > TamanhoMaximoPropriedade)
            throw new ArgumentException($"A propriedade deve ter até {TamanhoMaximoPropriedade} caracteres.", nameof(propriedade));

        Nome = nomeNormalizado;
        Propriedade = propriedadeNormalizada;
    }
}
