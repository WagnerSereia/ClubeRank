using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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
            var setsComparer = new ValueComparer<List<SetConfronto>>(
                (left, right) => ReferenceEquals(left, right) || (left != null && right != null && left.SequenceEqual(right)),
                sets => sets == null ? 0 : sets.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                sets => sets == null ? new List<SetConfronto>() : sets.Select(x => new SetConfronto(x.Numero, x.GamesAtletaA, x.GamesAtletaB, x.TieBreak)).ToList());

            resultado.Property(x => x.Tipo)
                .HasColumnName("TipoResultado")
                .HasConversion<string>()
                .HasMaxLength(30);
            resultado.Property(x => x.JustificativaWO)
                .HasColumnName("JustificativaWO")
                .HasMaxLength(1000);
            resultado.Property(x => x.DataRegistro)
                .HasColumnName("DataRegistroResultado");
            resultado.Property(x => x.Sets)
                .HasColumnName("SetsJson")
                .HasConversion(
                    sets => JsonSerializer.Serialize(sets, (JsonSerializerOptions?)null),
                    json => string.IsNullOrWhiteSpace(json)
                        ? new List<SetConfronto>()
                        : JsonSerializer.Deserialize<List<SetConfronto>>(json, (JsonSerializerOptions?)null) ?? new List<SetConfronto>())
                .Metadata.SetValueComparer(setsComparer);
        });
    }
}
