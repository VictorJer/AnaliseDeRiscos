using System.ComponentModel.DataAnnotations;
using ScoreRebanho.Web.Validacao;

namespace ScoreRebanho.Web.ViewModels.Rebanhos;

public sealed class SimulacaoViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe a área.")]
    [MaiorQueZero]
    [Display(Name = "Área de pastagem (ha)")]
    public decimal? AreaHectares { get; set; }

    [Required(ErrorMessage = "Selecione a pastagem.")]
    [Range(1, 2, ErrorMessage = "Selecione uma pastagem válida.")]
    [Display(Name = "Tipo de pastagem")]
    public int? Pastagem { get; set; }

    [Required(ErrorMessage = "Selecione a posse.")]
    [Range(1, 2, ErrorMessage = "Selecione um tipo de posse válido.")]
    [Display(Name = "Posse do terreno")]
    public int? Posse { get; set; }

    [Display(Name = "Arrendamento mensal (R$)")]
    public decimal? ArrendamentoMensalReais { get; set; }

    [Display(Name = "O pasto já está pronto")]
    public bool PastoPronto { get; set; } = true;
    [Display(Name = "Usar rotação de pastagem")]
    public bool RotacaoAtiva { get; set; }
    [Display(Name = "Número de piquetes")]
    public int? NumeroPiquetes { get; set; }

    [Required(ErrorMessage = "Informe a quantidade de animais.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma quantidade positiva.")]
    [Display(Name = "Quantidade de animais")]
    public int? QuantidadeAnimais { get; set; }

    [Required(ErrorMessage = "Informe o peso de compra.")]
    [MaiorQueZero]
    [Display(Name = "Peso médio de compra (kg/animal)")]
    public decimal? PesoCompraKg { get; set; }

    [Required(ErrorMessage = "Informe o preço de compra.")]
    [MaiorQueZero]
    [Display(Name = "Preço de compra por kg vivo (R$)")]
    public decimal? PrecoCompraPorKgReais { get; set; }

    [Required(ErrorMessage = "Informe o prazo.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma quantidade de dias positiva.")]
    [Display(Name = "Dias na pastagem")]
    public int? DiasPastagem { get; set; } = 180;

    [Display(Name = "Raça pura para corte")]
    public bool RacaPuraCorte { get; set; }
    [Display(Name = "Vacinação contra carbúnculo")]
    public bool VacinaCarbunculo { get; set; }
    [Display(Name = "Controle de carrapato")]
    public bool ControleCarrapato { get; set; }
    [Display(Name = "Uso de vermífugo")]
    public bool Vermifugo { get; set; }
    [Display(Name = "Uso de modificador orgânico")]
    public bool ModificadorOrganico { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Posse == 2 && ArrendamentoMensalReais is null)
            yield return new("Informe o arrendamento mensal.", [nameof(ArrendamentoMensalReais)]);
        if (ArrendamentoMensalReais < 0)
            yield return new("O arrendamento não pode ser negativo.", [nameof(ArrendamentoMensalReais)]);
        if (RotacaoAtiva && (NumeroPiquetes is null or <= 0))
            yield return new("Informe uma quantidade positiva de piquetes.", [nameof(NumeroPiquetes)]);
        else if (NumeroPiquetes < 0)
            yield return new("A quantidade de piquetes não pode ser negativa.", [nameof(NumeroPiquetes)]);
    }
}
