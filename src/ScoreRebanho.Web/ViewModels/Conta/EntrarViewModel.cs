using System.ComponentModel.DataAnnotations;

namespace ScoreRebanho.Web.ViewModels.Conta;

public sealed class EntrarViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(256, ErrorMessage = "O e-mail deve ter até 256 caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe sua senha.")]
    [StringLength(128, ErrorMessage = "A senha deve ter até 128 caracteres.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    [Display(Name = "Manter conectado")]
    public bool ManterConectado { get; set; }

    public string? ReturnUrl { get; set; }
}
