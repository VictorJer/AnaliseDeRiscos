using System.ComponentModel.DataAnnotations;

namespace ScoreRebanho.Web.ViewModels.Rebanhos;

public sealed class BuscaMunicipiosViewModel
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Nome { get; set; } = "";
}

public sealed class MunicipioViewModel
{
    public string Nome { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Pais { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
