namespace ScoreRebanho.Web.ViewModels.Acompanhamento;

public sealed record CondicaoTempoViewModel(string Descricao, string Icone)
{
    public static CondicaoTempoViewModel DoCodigo(int? codigo) => codigo switch
    {
        0 => new("Céu limpo", "sol"),
        1 => new("Predominantemente aberto", "sol-nuvem"),
        2 => new("Parcialmente nublado", "sol-nuvem"),
        3 => new("Nublado", "nuvem"),
        45 => new("Neblina", "neblina"),
        48 => new("Neblina com geada", "neblina"),
        51 => new("Garoa leve", "chuva"),
        53 => new("Garoa moderada", "chuva"),
        55 => new("Garoa intensa", "chuva"),
        56 or 57 => new("Garoa congelante", "chuva"),
        61 => new("Chuva leve", "chuva"),
        63 => new("Chuva moderada", "chuva"),
        65 => new("Chuva forte", "chuva"),
        66 or 67 => new("Chuva congelante", "chuva"),
        71 => new("Neve leve", "neve"),
        73 => new("Neve moderada", "neve"),
        75 => new("Neve intensa", "neve"),
        77 => new("Grãos de neve", "neve"),
        80 => new("Pancadas de chuva leves", "chuva"),
        81 => new("Pancadas de chuva moderadas", "chuva"),
        82 => new("Pancadas de chuva intensas", "chuva"),
        85 or 86 => new("Pancadas de neve", "neve"),
        95 => new("Tempestade", "tempestade"),
        97 => new("Tempestade forte", "tempestade"),
        96 or 99 => new("Tempestade com granizo", "tempestade"),
        _ => new("Condição não informada", "indisponivel")
    };
}
