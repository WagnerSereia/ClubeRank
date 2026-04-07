using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.OrganizacaoId);
        builder.Property(x => x.UsuarioId);
        builder.Property(x => x.Entidade).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntidadeId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Acao).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DadosAntes);
        builder.Property(x => x.DadosDepois);
        builder.Property(x => x.Ip).HasMaxLength(50);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.HasIndex(x => new { x.TenantId, x.DataCriacao });
        builder.HasIndex(x => new { x.TenantId, x.Entidade, x.Acao });
    }
}
