using ScoreRebanho.Web.ViewModels.Relatorios;

namespace ScoreRebanho.Web.ViewModels.Administracao;

public sealed record ListaScoresAdministrativosViewModel(int Pagina, int TamanhoPagina, int Total,
    List<ItemScoreAdministrativoViewModel> Itens)
{
    public int Paginas => Math.Max(1, (int)Math.Ceiling((decimal)Total / TamanhoPagina));
}

public sealed record ItemScoreAdministrativoViewModel(Guid Id, string AutorId, string? AutorEmail,
    string? NomeRebanho, string? Propriedade, DateTimeOffset CalculadoEmUtc,
    string VersaoPremissas, int Score, string Classificacao);

public sealed record RelatorioScoreAdministrativoViewModel(Guid Id, string AutorId, string? AutorEmail,
    string? NomeRebanho, string? Propriedade, AnaliseCompletaViewModel Analise);
