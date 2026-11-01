using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Infrastructure.Persistencia.Rebanhos;
using ScoreRebanho.Infrastructure.Persistencia.Historico;

namespace ScoreRebanho.Infrastructure.Persistencia;

public sealed class ContasDbContext(DbContextOptions<ContasDbContext> options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new RebanhoConfiguracao());
        builder.ApplyConfiguration(new ScoreHistoricoConfiguracao());
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUser>()
            .HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ProtegerHistorico();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ProtegerHistorico();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ProtegerHistorico()
    {
        if (ChangeTracker.Entries<ScoreHistoricoRegistro>().Any(e => e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Scores históricos são imutáveis.");
    }
}
