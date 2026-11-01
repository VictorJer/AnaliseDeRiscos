using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Infrastructure.Persistencia.Historico;

internal sealed class ScoreHistoricoConfiguracao : IEntityTypeConfiguration<ScoreHistoricoRegistro>
{
    public void Configure(EntityTypeBuilder<ScoreHistoricoRegistro> builder)
    {
        builder.ToTable("ScoresHistoricos");
        builder.HasKey(s => s.Id);
        builder.HasAlternateKey(s => new { s.Id, s.AutorId });
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.AutorId).IsRequired();
        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(s => s.AutorId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.NomeRebanho).HasMaxLength(IdentificacaoRebanho.TamanhoMaximoNome);
        builder.Property(s => s.Propriedade).HasMaxLength(IdentificacaoRebanho.TamanhoMaximoPropriedade);
        builder.Property(s => s.CalculadoEmUtc).HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));
        builder.Property(s => s.VersaoPremissas).IsRequired();
        builder.Property(s => s.AnaliseJson).IsRequired();
        builder.HasIndex(s => new { s.AutorId, s.CalculadoEmUtc, s.Id });
        builder.HasIndex(s => new { s.CalculadoEmUtc, s.Id });
        builder.HasIndex(s => s.RebanhoOrigemId);
    }
}
