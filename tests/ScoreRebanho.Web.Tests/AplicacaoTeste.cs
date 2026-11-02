using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Web.Tests;

internal sealed class AplicacaoTeste : WebApplicationFactory<Program>
{
    internal Action<IServiceCollection>? ConfigurarServicos { get; init; }
    private readonly string banco = Path.Combine(Path.GetTempPath(), $"score-rebanho-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ContasDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ContasDbContext>>();
            services.AddDbContext<ContasDbContext>(options =>
                options.UseSqlite($"Data Source={banco};Pooling=False"));
            ConfigurarServicos?.Invoke(services);
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["ConnectionStrings:Contas"] = $"Data Source={banco};Pooling=False"
            }));
    }

    public HttpClient CriarCliente() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        foreach (var arquivo in new[] { banco, banco + "-wal", banco + "-shm" })
            if (File.Exists(arquivo))
                File.Delete(arquivo);
    }
}
