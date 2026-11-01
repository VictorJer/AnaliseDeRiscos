using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ScoreRebanho.Web.ViewModels.Rebanhos;

public sealed class RebanhoFormularioViewModel
{
    public Guid Id { get; set; }
    public Guid Versao { get; set; }
    [Required(ErrorMessage = "Informe o nome do rebanho.")]
    [StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres.")]
    [Display(Name = "Nome do rebanho")]
    public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Informe a propriedade.")]
    [StringLength(150, ErrorMessage = "A propriedade deve ter até 150 caracteres.")]
    [Display(Name = "Propriedade")]
    public string Propriedade { get; set; } = "";
    [Required(ErrorMessage = "Informe os dados da simulação.")]
    public SimulacaoViewModel Entradas { get; set; } = new();
    [BindNever]
    public ConferenciaOcupacaoViewModel? Conferencia { get; set; }
}

public sealed class ConferenciaOcupacaoViewModel
{
    public string VersaoPremissas { get; set; } = "";
    public decimal OcupacaoDecimal { get; set; }
    public decimal CapacidadeUa { get; set; }
    public decimal QuantidadeMaximaAnimais { get; set; }
}
