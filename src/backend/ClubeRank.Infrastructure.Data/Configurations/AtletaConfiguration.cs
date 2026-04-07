using ClubeRank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubeRank.Infrastructure.Data.Configurations;

public class AtletaConfiguration : IEntityTypeConfiguration<Atleta>
{
    public void Configure(EntityTypeBuilder<Atleta> builder)
    {
        builder.ToTable("Atletas");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Genero).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrganizacaoId).IsRequired();
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.DataAtualizacao);

        builder.OwnsOne(x => x.Nome, nome =>
        {
            nome.Property(x => x.PrimeiroNome)
                .HasColumnName("PrimeiroNome")
                .HasMaxLength(100)
                .IsRequired();
            nome.Property(x => x.Sobrenome)
                .HasColumnName("Sobrenome")
                .HasMaxLength(150)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Valor)
                .HasColumnName("EmailValor")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(x => x.PontuacaoAtual, pontuacao =>
        {
            pontuacao.Property(x => x.Valor)
                .HasColumnName("PontuacaoAtual")
                .IsRequired();
            pontuacao.Property(x => x.DataAtualizacao)
                .HasColumnName("DataAtualizacaoPontuacao")
                .IsRequired();
        });

    }
}
