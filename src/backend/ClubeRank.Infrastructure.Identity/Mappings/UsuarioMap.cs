using ClubeRank.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Identity.Mappings;

public class UsuarioMap : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.NomeCompleto)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasMany(x => x.UsuarioClubes)
            .WithOne(x => x.Usuario)
            .HasForeignKey(x => x.UsuarioId);
    }
}
