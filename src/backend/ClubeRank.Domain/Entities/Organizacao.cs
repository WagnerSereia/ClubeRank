using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Entities;

public class Organizacao : Entity
{
    public string Nome { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string Modalidade { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }
    public StatusOrganizacao Status { get; private set; }
    public TipoPlano Plano { get; private set; }
    public ConfiguracaoOrganizacao Configuracao { get; private set; } = null!;

    private Organizacao() { } // Para EF Core

    public Organizacao(string nome, Email email, string? telefone, string modalidade, TipoPlano plano)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Telefone = telefone;
        Modalidade = modalidade ?? throw new ArgumentNullException(nameof(modalidade));
        Status = StatusOrganizacao.Ativa;
        Plano = plano;
        Configuracao = new ConfiguracaoOrganizacao();
        TenantId = Id; // Cada organização é um tenant
    }

    public void AtualizarDados(string nome, string? telefone, string? logoUrl)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Telefone = telefone;
        LogoUrl = logoUrl;
        Atualizar();
    }

    public void AlterarPlano(TipoPlano plano)
    {
        Plano = plano;
        Atualizar();
    }

    public void AtualizarConfiguracao(ConfiguracaoOrganizacao configuracao)
    {
        Configuracao = configuracao ?? throw new ArgumentNullException(nameof(configuracao));
        Atualizar();
    }

    public void Suspender()
    {
        Status = StatusOrganizacao.Suspensa;
        Atualizar();
    }

    public void Reativar()
    {
        Status = StatusOrganizacao.Ativa;
        Atualizar();
    }

    public void Inativar()
    {
        Status = StatusOrganizacao.Inativa;
        Atualizar();
    }
}

public class ConfiguracaoOrganizacao : ValueObject
{
    public int PontuacaoVitoria { get; private set; }
    public int PontuacaoDerrota { get; private set; }
    public int PontuacaoEmpate { get; private set; }
    public int PontuacaoWO { get; private set; }
    public int PontuacaoInicial { get; private set; }
    public int PontuacaoMinima { get; private set; }
    public int PontuacaoMaxima { get; private set; }
    public int DiasDecaimento { get; private set; }
    public int PontuacaoDecaimentoSemanal { get; private set; }
    public int PontuacaoPisoDecaimento { get; private set; }

    public ConfiguracaoOrganizacao()
    {
        PontuacaoVitoria = 10;
        PontuacaoDerrota = -5;
        PontuacaoEmpate = 0;
        PontuacaoWO = -20;
        PontuacaoInicial = 1000;
        PontuacaoMinima = 0;
        PontuacaoMaxima = 5000;
        DiasDecaimento = 30;
        PontuacaoDecaimentoSemanal = -10;
        PontuacaoPisoDecaimento = 900;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PontuacaoVitoria;
        yield return PontuacaoDerrota;
        yield return PontuacaoEmpate;
        yield return PontuacaoWO;
        yield return PontuacaoInicial;
        yield return PontuacaoMinima;
        yield return PontuacaoMaxima;
        yield return DiasDecaimento;
        yield return PontuacaoDecaimentoSemanal;
        yield return PontuacaoPisoDecaimento;
    }
}
