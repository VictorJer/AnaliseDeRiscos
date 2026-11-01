using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ScoreRebanho.Web.ViewModels.Rebanhos;

public sealed class SimulacaoFormularioViewModel
{
    [Required(ErrorMessage = "Informe os dados da simulação.")]
    public SimulacaoViewModel Entradas { get; set; } = new();
    [BindNever]
    public ConferenciaOcupacaoViewModel? Conferencia { get; set; }
}
