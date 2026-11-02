using ScoreRebanho.Application.Administracao;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Infrastructure.Persistencia.Rebanhos;
using ScoreRebanho.Infrastructure.Persistencia.Historico;
using ScoreRebanho.Infrastructure.Identidade;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Infrastructure;

public static class DependenciasInfrastructure
{
    public static IServiceCollection AdicionarConsultaMunicipios(this IServiceCollection servicos)
    {
        servicos.AddHttpClient<ScoreRebanho.Application.Localizacao.IConsultaMunicipios, ScoreRebanho.Infrastructure.Localizacao.OpenMeteoMunicipios>(cliente =>
        {
            cliente.BaseAddress = new Uri("https://geocoding-api.open-meteo.com/");
            cliente.Timeout = TimeSpan.FromSeconds(10);
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd("ScoreRebanho-Demonstracao/1.0");
        });
        return servicos;
    }

    public static IdentityBuilder AdicionarPersistenciaContas(this IServiceCollection servicos,
        string conexao, Action<IdentityOptions> configurarIdentity)
    {
        servicos.AddDbContext<ContasDbContext>(options => options.UseSqlite(conexao));
        servicos.AddScoped<ICadastroContas, CadastroContas>();
        servicos.AddScoped<IRepositorioRebanhos, RepositorioRebanhos>();
        servicos.AddScoped<IRepositorioScores, RepositorioScores>();
        servicos.AddScoped<IConsultaScoresAdministrativos, ConsultaScoresAdministrativos>();
        return servicos.AddIdentity<IdentityUser, IdentityRole>(configurarIdentity)
            .AddEntityFrameworkStores<ContasDbContext>();
    }

    public static async Task MigrarContasAsync(this IServiceProvider servicos, CancellationToken cancellationToken = default)
    {
        await using var escopo = servicos.CreateAsyncScope();
        await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
