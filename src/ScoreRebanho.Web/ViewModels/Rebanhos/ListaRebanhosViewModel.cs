using System.ComponentModel.DataAnnotations;

namespace ScoreRebanho.Web.ViewModels.Rebanhos;

public sealed class PaginacaoViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma página maior que zero.")]
    public int Pagina { get; set; } = 1;
}

public sealed class ListaRebanhosViewModel
{
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int Total { get; set; }
    public List<ItemListaRebanhosViewModel> Itens { get; set; } = [];
    public int Paginas => TamanhoPagina > 0 ? Total / TamanhoPagina + (Total % TamanhoPagina > 0 ? 1 : 0) : 0;
}

public sealed class ItemListaRebanhosViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
    public DateTimeOffset AtualizadoEmUtc { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Score { get; set; }
    public string Classificacao { get; set; } = "";
}

public sealed class ExcluirRebanhoViewModel
{
    public Guid Id { get; set; }
    public Guid Versao { get; set; }
}

public sealed class ConfirmacaoExclusaoViewModel
{
    public Guid Id { get; set; }
    public Guid Versao { get; set; }
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
}
