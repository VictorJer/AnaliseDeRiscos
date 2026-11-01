using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Infrastructure.Identidade;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Infrastructure;

public static class DependenciasInfrastructure
{
    public static IdentityBuilder AdicionarPersistenciaContas(this IServiceCollection servicos,
        string conexao, Action<IdentityOptions> configurarIdentity)
    {
        servicos.AddDbContext<ContasDbContext>(options => options.UseSqlite(conexao));
        servicos.AddScoped<ICadastroContas, CadastroContas>();
        return servicos.AddIdentity<IdentityUser, IdentityRole>(configurarIdentity)
            .AddEntityFrameworkStores<ContasDbContext>();
    }

    public static async Task MigrarContasAsync(this IServiceProvider servicos, CancellationToken cancellationToken = default)
    {
        await using var escopo = servicos.CreateAsyncScope();
        await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
