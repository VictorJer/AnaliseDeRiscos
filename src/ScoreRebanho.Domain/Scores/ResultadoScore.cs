namespace ScoreRebanho.Domain.Scores;

public sealed class ResultadoScore
{
    public const decimal Maximo = 100m;
    public const int LimiteRiscoBaixo = 80;
    public const int LimiteRiscoModerado = 60;
    public const int LimiteRiscoAlto = 40;

    public string VersaoPremissas { get; }
    public IReadOnlyList<ComponenteScore> Componentes { get; }
    public int Total { get; }
    public ClassificacaoRisco Classificacao { get; }

    internal ResultadoScore(string versao, IEnumerable<ComponenteScore> componentes)
    {
        VersaoPremissas = versao;
        Componentes = Array.AsReadOnly(componentes.ToArray());
        var limitado = Math.Clamp(Componentes.Sum(c => c.Pontos), 0m, Maximo);
        Total = (int)decimal.Round(limitado, 0, MidpointRounding.AwayFromZero);
        Classificacao = Total switch
        {
            >= LimiteRiscoBaixo => ClassificacaoRisco.Baixo,
            >= LimiteRiscoModerado => ClassificacaoRisco.Moderado,
            >= LimiteRiscoAlto => ClassificacaoRisco.Alto,
            _ => ClassificacaoRisco.MuitoAlto
        };
    }
}
