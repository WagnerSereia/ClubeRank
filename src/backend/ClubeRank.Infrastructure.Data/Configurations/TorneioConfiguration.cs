using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class TorneioConfiguration : IEntityTypeConfiguration<Torneio>
{
    public void Configure(EntityTypeBuilder<Torneio> builder)
    {
        builder.ToTable("Torneios");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(1000);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.OrganizacaoId).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.OwnsOne(x => x.Configuracao, config =>
        {
            config.Property(x => x.PontuacaoVitoria).HasColumnName("PontuacaoVitoria");
            config.Property(x => x.PontuacaoDerrota).HasColumnName("PontuacaoDerrota");
            config.Property(x => x.PontuacaoEmpate).HasColumnName("PontuacaoEmpate");
            config.Property(x => x.PontuacaoWO).HasColumnName("PontuacaoWO");
            config.Property(x => x.PontuacaoSetVencido).HasColumnName("PontuacaoSetVencido");
            config.Property(x => x.MelhorDeSets).HasColumnName("MelhorDeSets");
            config.Property(x => x.PermiteEmpate).HasColumnName("PermiteEmpate");
        });

        builder.HasMany(x => x.Atletas)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TorneioAtleta",
                join => join
                    .HasOne<Atleta>()
                    .WithMany()
                    .HasForeignKey("AtletaId")
                    .OnDelete(DeleteBehavior.Cascade),
                join => join
                    .HasOne<Torneio>()
                    .WithMany()
                    .HasForeignKey("TorneioId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("TorneioAtletas");
                    join.HasKey("TorneioId", "AtletaId");
                });
    }
}
