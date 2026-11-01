using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScoreRebanho.Domain.Rebanhos;
using ScoreRebanho.Infrastructure.Persistencia.Historico;

namespace ScoreRebanho.Infrastructure.Persistencia.Rebanhos;

internal sealed class RebanhoConfiguracao : IEntityTypeConfiguration<Rebanho>
{
    public void Configure(EntityTypeBuilder<Rebanho> builder)
    {
        builder.ToTable("Rebanhos");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.AutorId).IsRequired().IsConcurrencyToken();
        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(r => r.AutorId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Versao).IsConcurrencyToken();
        builder.Property(r => r.CriadoEmUtc).HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));
        builder.Property(r => r.AtualizadoEmUtc).HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));
        builder.HasIndex(r => new { r.AutorId, r.AtualizadoEmUtc, r.Id });
        builder.HasOne<ScoreHistoricoRegistro>().WithMany().HasForeignKey(r => new { r.UltimoScoreId, r.AutorId }).HasPrincipalKey(s => new { s.Id, s.AutorId }).OnDelete(DeleteBehavior.Restrict);
        builder.OwnsOne(r => r.Identificacao, identificacao =>
        {
            identificacao.Property(i => i.Nome).HasColumnName("Nome").HasMaxLength(IdentificacaoRebanho.TamanhoMaximoNome).IsRequired();
            identificacao.Property(i => i.Propriedade).HasColumnName("Propriedade").HasMaxLength(IdentificacaoRebanho.TamanhoMaximoPropriedade).IsRequired();
        });
        builder.Navigation(r => r.Identificacao).IsRequired();
    }
}
