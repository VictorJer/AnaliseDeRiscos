using System.Globalization;

namespace ScoreRebanho.Web.ViewModels.Relatorios;

public static class FormatoRelatorio
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");
    public static string Numero(decimal? valor, int casas = 2) => valor?.ToString($"N{casas}", Cultura) ?? "Indisponível";
    public static string Moeda(decimal? valor) => valor?.ToString("C2", Cultura) ?? "Indisponível";
    public static string Percentual(decimal? valor) => valor?.ToString("P2", Cultura) ?? "Indisponível";
    public static string Data(DateTimeOffset data) => data.UtcDateTime.ToString("dd/MM/yyyy HH:mm 'UTC'", Cultura);
    public static string SimNao(bool valor) => valor ? "Sim" : "Não";
    public static string Risco(string codigo) => codigo switch
    {
        "Baixo" => "Risco baixo", "Moderado" => "Risco moderado",
        "Alto" => "Risco alto", "MuitoAlto" => "Risco muito alto", _ => "Classificação indisponível"
    };
}
