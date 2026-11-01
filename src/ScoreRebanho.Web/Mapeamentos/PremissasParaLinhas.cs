using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Web.ViewModels.Relatorios;

namespace ScoreRebanho.Web.Mapeamentos;

// Converte somente valores de apresentação do snapshot; não interpreta nem executa fórmulas.
public sealed class PremissasParaLinhas : ITypeConverter<PremissasUtilizadasDto, List<ParametroViewModel>>
{
    public List<ParametroViewModel> Convert(PremissasUtilizadasDto source, List<ParametroViewModel> destination, ResolutionContext context)
    {
        var linhas = new List<ParametroViewModel>();
        Adicionar(source, "", linhas);
        return linhas;
    }

    private static void Adicionar(object origem, string prefixo, List<ParametroViewModel> linhas)
    {
        foreach (var propriedade in origem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var valor = propriedade.GetValue(origem);
            var nome = prefixo + Regex.Replace(propriedade.Name, "(?<=[a-z])(?=[A-Z])", " ");
            if (valor is null)
                linhas.Add(new(nome, "Não se aplica"));
            else if (valor is string or decimal or int or bool)
                linhas.Add(new(nome, System.Convert.ToString(valor, CultureInfo.GetCultureInfo("pt-BR")) ?? ""));
            else
                Adicionar(valor, nome + " / ", linhas);
        }
    }
}
