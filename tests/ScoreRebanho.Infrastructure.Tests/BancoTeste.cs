using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Infrastructure.Tests;

internal sealed class BancoTeste : IAsyncDisposable
{
    private readonly SqliteConnection ancora;
    internal ServiceProvider Servicos { get; }

    private BancoTeste(SqliteConnection ancora, ServiceProvider servicos)
    {
        this.ancora = ancora;
        Servicos = servicos;
    }

    internal static async Task<BancoTeste> CriarAsync()
    {
        var conexao = new SqliteConnectionStringBuilder
        {
            DataSource = $"teste-{Guid.NewGuid():N}", Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared, ForeignKeys = true, Pooling = false
        }.ToString();
        var ancora = new SqliteConnection(conexao);
        await ancora.OpenAsync();
        var servicos = new ServiceCollection();
        servicos.AddLogging();
        servicos.AdicionarPersistenciaContas(conexao, options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version1;
            options.Stores.MaxLengthForKeys = 128;
        });
        var banco = new BancoTeste(ancora, servicos.BuildServiceProvider());
        try
        {
            await using var escopo = banco.Servicos.CreateAsyncScope();
            await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Database.MigrateAsync();
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            foreach (var autor in new[] { "autor-a", "autor-b" })
            {
                var email = $"{autor}@example.com";
                var resultado = await usuarios.CreateAsync(new IdentityUser { Id = autor, UserName = email, Email = email });
                Assert.IsTrue(resultado.Succeeded);
            }
            return banco;
        }
        catch
        {
            await banco.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Servicos.DisposeAsync();
        await ancora.DisposeAsync();
    }
}
