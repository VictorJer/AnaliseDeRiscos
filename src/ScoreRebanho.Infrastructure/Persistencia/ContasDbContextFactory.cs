using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ScoreRebanho.Infrastructure.Persistencia;

public sealed class ContasDbContextFactory : IDesignTimeDbContextFactory<ContasDbContext>
{
    public ContasDbContext CreateDbContext(string[] args)
    {
        var servicos = new ServiceCollection();
        servicos.Configure<IdentityOptions>(options =>
        {
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version1;
            options.Stores.MaxLengthForKeys = 128;
        });
        var options = new DbContextOptionsBuilder<ContasDbContext>()
            .UseApplicationServiceProvider(servicos.BuildServiceProvider())
            .UseSqlite("Data Source=:memory:").Options;
        return new(options);
    }
}
