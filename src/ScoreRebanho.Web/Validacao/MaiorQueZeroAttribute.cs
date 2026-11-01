using System.ComponentModel.DataAnnotations;

namespace ScoreRebanho.Web.Validacao;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MaiorQueZeroAttribute : ValidationAttribute
{
    public MaiorQueZeroAttribute() => ErrorMessage = "Informe um valor maior que zero.";
    public override bool IsValid(object? value) => value is null || value is decimal numero && numero > 0m;
}
