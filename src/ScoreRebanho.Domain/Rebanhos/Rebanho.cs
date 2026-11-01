namespace ScoreRebanho.Domain.Rebanhos;

public sealed class Rebanho
{
    public Guid Id { get; private set; }
    public string AutorId { get; private set; } = "";
    public IdentificacaoRebanho Identificacao { get; private set; } = null!;
    public Guid UltimoScoreId { get; private set; }
    public Guid Versao { get; private set; }
    public DateTimeOffset CriadoEmUtc { get; private set; }
    public DateTimeOffset AtualizadoEmUtc { get; private set; }

    private Rebanho() { }

    public static Rebanho Criar(string autorId, IdentificacaoRebanho identificacao,
        Guid scoreId, DateTimeOffset calculadoEm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentNullException.ThrowIfNull(identificacao);
        if (scoreId == Guid.Empty)
            throw new ArgumentException("Informe o identificador do score.", nameof(scoreId));
        var data = calculadoEm.ToUniversalTime();
        return new()
        {
            Id = Guid.NewGuid(), AutorId = autorId, Identificacao = identificacao,
            UltimoScoreId = scoreId, Versao = Guid.NewGuid(), CriadoEmUtc = data, AtualizadoEmUtc = data
        };
    }

    public void Atualizar(IdentificacaoRebanho identificacao, Guid scoreId, DateTimeOffset calculadoEm)
    {
        ArgumentNullException.ThrowIfNull(identificacao);
        if (scoreId == Guid.Empty)
            throw new ArgumentException("Informe o identificador do score.", nameof(scoreId));
        Identificacao = identificacao;
        UltimoScoreId = scoreId;
        AtualizadoEmUtc = calculadoEm.ToUniversalTime();
        Versao = Guid.NewGuid();
    }
}
