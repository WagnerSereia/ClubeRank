using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class ConfrontoConfiguration : IEntityTypeConfiguration<Confronto>
{
    public void Configure(EntityTypeBuilder<Confronto> builder)
    {
        builder.ToTable("Confrontos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.AtletaAId).IsRequired();
        builder.Property(x => x.AtletaBId).IsRequired();
        builder.Property(x => x.TorneioId).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.DataAgendamento).IsRequired();
        builder.Property(x => x.Notas).HasMaxLength(1000);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.HasOne(x => x.AtletaA)
            .WithMany()
            .HasForeignKey(x => x.AtletaAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AtletaB)
            .WithMany()
            .HasForeignKey(x => x.AtletaBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Torneio)
            .WithMany()
            .HasForeignKey(x => x.TorneioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.Resultado, resultado =>
        {
            resultado.Property(x => x.Tipo)
                .HasColumnName("TipoResultado")
                .HasConversion<string>()
                .HasMaxLength(30);
            resultado.Property(x => x.JustificativaWO)
                .HasColumnName("JustificativaWO")
                .HasMaxLength(1000);
            resultado.Property(x => x.DataRegistro)
                .HasColumnName("DataRegistroResultado");
        });
    }
}
