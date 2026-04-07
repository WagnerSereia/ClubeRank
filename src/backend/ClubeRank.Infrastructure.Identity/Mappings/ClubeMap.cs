using ClubeRank.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Identity.Mappings;

public class ClubeMap : IEntityTypeConfiguration<Clube>
{
    public void Configure(EntityTypeBuilder<Clube> builder)
    {
        builder.ToTable("Clubes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Nome)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasMany(x => x.UsuarioClubes)
            .WithOne(x => x.Clube)
            .HasForeignKey(x => x.ClubeId);
    }
}
