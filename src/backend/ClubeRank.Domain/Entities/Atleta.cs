using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Entities;

public class Atleta : Entity
{
    public NomeCompleto Nome { get; private set; } = null!;
    public Genero Genero { get; private set; }
    public Email Email { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public StatusAtleta Status { get; private set; }
    public Categoria Categoria { get; private set; }
    public Pontuacao PontuacaoAtual { get; private set; } = null!;
    public Guid OrganizacaoId { get; private set; }

    private Atleta() { } // Para EF Core

    public Atleta(NomeCompleto nome, Genero genero, Email email, string? telefone, Categoria categoria, Guid organizacaoId)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Genero = genero;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Telefone = telefone;
        Status = StatusAtleta.Ativo;
        Categoria = categoria;
        PontuacaoAtual = new Pontuacao(1000); // Pontuação inicial padrão
        OrganizacaoId = organizacaoId;
        TenantId = organizacaoId; // Multi-tenant por organização
    }

    public void AlterarDadosPessoais(NomeCompleto nome, string? telefone)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Telefone = telefone;
        Atualizar();
    }

    public void AlterarEmail(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Atualizar();
    }

    public void AlterarCategoria(Categoria categoria)
    {
        Categoria = categoria;
        Atualizar();
    }

    public void Ativar()
    {
        Status = StatusAtleta.Ativo;
        Atualizar();
    }

    public void Inativar()
    {
        Status = StatusAtleta.Inativo;
        Atualizar();
    }

    public void Suspender()
    {
        Status = StatusAtleta.Suspenso;
        Atualizar();
    }

    public void AtualizarPontuacao(int novaPontuacao)
    {
        PontuacaoAtual = new Pontuacao(novaPontuacao);
        Atualizar();
    }
}
