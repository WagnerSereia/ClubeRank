using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class OrganizacaoConfiguration : IEntityTypeConfiguration<Organizacao>
{
    public void Configure(EntityTypeBuilder<Organizacao> builder)
    {
        builder.ToTable("Organizacoes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.Property(x => x.Modalidade).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Plano).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Valor)
                .HasColumnName("Email")
                .HasMaxLength(200)
                .IsRequired();
            email.HasIndex(x => x.Valor).IsUnique();
        });

        builder.OwnsOne(x => x.Configuracao, config =>
        {
            config.Property(x => x.PontuacaoVitoria).HasColumnName("PontuacaoVitoria");
            config.Property(x => x.PontuacaoDerrota).HasColumnName("PontuacaoDerrota");
            config.Property(x => x.PontuacaoEmpate).HasColumnName("PontuacaoEmpate");
            config.Property(x => x.PontuacaoWO).HasColumnName("PontuacaoWO");
            config.Property(x => x.PontuacaoInicial).HasColumnName("PontuacaoInicial");
            config.Property(x => x.PontuacaoMinima).HasColumnName("PontuacaoMinima");
            config.Property(x => x.PontuacaoMaxima).HasColumnName("PontuacaoMaxima");
            config.Property(x => x.DiasDecaimento).HasColumnName("DiasDecaimento");
            config.Property(x => x.PontuacaoDecaimentoSemanal).HasColumnName("PontuacaoDecaimentoSemanal");
            config.Property(x => x.PontuacaoPisoDecaimento).HasColumnName("PontuacaoPisoDecaimento");
        });
    }
}
