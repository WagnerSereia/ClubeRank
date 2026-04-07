using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class HistoricoRankingConfiguration : IEntityTypeConfiguration<HistoricoRanking>
{
    public void Configure(EntityTypeBuilder<HistoricoRanking> builder)
    {
        builder.ToTable("HistoricosRanking");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.AtletaId).IsRequired();
        builder.Property(x => x.ConfrontoId);
        builder.Property(x => x.UsuarioId).IsRequired();
        builder.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.HasOne(x => x.Atleta)
            .WithMany()
            .HasForeignKey(x => x.AtletaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Confronto)
            .WithMany()
            .HasForeignKey(x => x.ConfrontoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.PontuacaoAntes, pontuacao =>
        {
            pontuacao.Property(x => x.Valor)
                .HasColumnName("PontuacaoAntes")
                .IsRequired();
            pontuacao.Property(x => x.DataAtualizacao)
                .HasColumnName("DataPontuacaoAntes")
                .IsRequired();
        });

        builder.OwnsOne(x => x.PontuacaoDepois, pontuacao =>
        {
            pontuacao.Property(x => x.Valor)
                .HasColumnName("PontuacaoDepois")
                .IsRequired();
            pontuacao.Property(x => x.DataAtualizacao)
                .HasColumnName("DataPontuacaoDepois")
                .IsRequired();
        });
    }
}
