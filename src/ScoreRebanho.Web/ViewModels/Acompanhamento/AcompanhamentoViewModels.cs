namespace ScoreRebanho.Web.ViewModels.Acompanhamento;

public sealed class ConsultaPrevisaoViewModel
{
    public Guid Id { get; set; }
}

public sealed class PaginaAcompanhamentoViewModel
{
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int Total { get; set; }
    public List<RebanhoAcompanhamentoViewModel> Itens { get; set; } = [];
    public int Paginas => TamanhoPagina > 0 ? Total / TamanhoPagina + (Total % TamanhoPagina > 0 ? 1 : 0) : 0;
}

public sealed class RebanhoAcompanhamentoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool TemLocalizacao => Latitude.HasValue && Longitude.HasValue;
}

public sealed class PrevisaoRebanhoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public PrevisaoViewModel Previsao { get; set; } = new();
}

public sealed class PrevisaoViewModel
{
    public DateTimeOffset ConsultadaEmUtc { get; set; }
    public string FusoHorario { get; set; } = "";
    public List<DiaPrevisaoViewModel> Dias { get; set; } = [];
}

public sealed class DiaPrevisaoViewModel
{
    public int? CodigoTempo { get; set; }
    public CondicaoTempoViewModel Condicao => CondicaoTempoViewModel.DoCodigo(CodigoTempo);
    public DateOnly Data { get; set; }
    public decimal TemperaturaMinimaCelsius { get; set; }
    public decimal TemperaturaMaximaCelsius { get; set; }
    public decimal PrecipitacaoMilimetros { get; set; }
    public decimal VentoMaximoQuilometrosHora { get; set; }
}

public sealed class FalhaPrevisaoViewModel
{
    public Guid Id { get; set; }
    public string Mensagem { get; set; } = "";
    public bool SemLocalizacao { get; set; }
}
