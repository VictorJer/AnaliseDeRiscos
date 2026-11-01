using System.Reflection;
using Microsoft.Extensions.Configuration;
using ScoreRebanho.Domain.Analises;

namespace ScoreRebanho.Infrastructure.Configuracao;

public static class CarregarPremissas
{
    public static PremissasAnalise DaConfiguracao(IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(configuracao);
        try
        {
            return configuracao.GetRequiredSection("Premissas")
                .Get<PremissasAnalise>(opcoes => opcoes.ErrorOnUnknownConfiguration = true)
                ?? throw new InvalidOperationException("Informe o conjunto de premissas.");
        }
        catch (Exception erro) when (erro is InvalidOperationException or ArgumentException or OverflowException
            or TargetInvocationException { InnerException: ArgumentException or OverflowException or InvalidOperationException })
        {
            throw new InvalidOperationException(
                "A configuração de Premissas é inválida. Confira parâmetros, unidades e versões antes de iniciar a aplicação.", erro);
        }
    }
}
