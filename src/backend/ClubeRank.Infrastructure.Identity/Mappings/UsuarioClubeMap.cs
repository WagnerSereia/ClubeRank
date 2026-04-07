using ClubeRank.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Identity.Mappings;

public class UsuarioClubeMap : IEntityTypeConfiguration<UsuarioClube>
{
    public void Configure(EntityTypeBuilder<UsuarioClube> builder)
    {
        builder.ToTable("UsuariosClubes");

        builder.HasKey(x => new { x.UsuarioId, x.ClubeId });

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuarioClubes)
            .HasForeignKey(x => x.UsuarioId);

        builder.HasOne(x => x.Clube)
            .WithMany(x => x.UsuarioClubes)
            .HasForeignKey(x => x.ClubeId);
    }
}
