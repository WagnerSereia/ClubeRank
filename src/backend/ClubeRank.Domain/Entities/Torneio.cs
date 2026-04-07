using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Entities;

public class Torneio : Entity
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public TipoTorneio Tipo { get; private set; }
    public StatusTorneio Status { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public Guid OrganizacaoId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public ConfiguracaoTorneio Configuracao { get; private set; } = null!;

    private readonly List<Atleta> _atletas = new();
    public IReadOnlyCollection<Atleta> Atletas => _atletas.AsReadOnly();

    private Torneio() { } // Para EF Core

    public Torneio(string nome, TipoTorneio tipo, Guid organizacaoId, Categoria? categoria = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Tipo = tipo;
        Status = StatusTorneio.Planejado;
        OrganizacaoId = organizacaoId;
        Categoria = categoria;
        Configuracao = new ConfiguracaoTorneio();
        TenantId = organizacaoId;
    }

    public void AtualizarDados(string nome, string? descricao, DateTime? dataInicio, DateTime? dataFim)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Atualizar();
    }

    public void AtualizarConfiguracaoPontuacao(
        int? pontuacaoVitoria,
        int? pontuacaoDerrota,
        int? pontuacaoEmpate,
        int? pontuacaoWo,
        bool permiteEmpate)
    {
        Configuracao = Configuracao.Atualizar(
            pontuacaoVitoria,
            pontuacaoDerrota,
            pontuacaoEmpate,
            pontuacaoWo,
            permiteEmpate);

        Atualizar();
    }

    public void Iniciar()
    {
        if (Status != StatusTorneio.Planejado)
            throw new InvalidOperationException("Apenas torneios planejados podem ser iniciados");

        if (_atletas.Count < 2)
            throw new InvalidOperationException("Torneio deve ter pelo menos 2 atletas");

        Status = StatusTorneio.Ativo;
        DataInicio = DateTime.UtcNow;
        Atualizar();
    }

    public void Encerrar()
    {
        if (Status != StatusTorneio.Ativo)
            throw new InvalidOperationException("Apenas torneios ativos podem ser encerrados");

        Status = StatusTorneio.Encerrado;
        DataFim = DateTime.UtcNow;
        Atualizar();
    }

    public void Cancelar()
    {
        if (Status == StatusTorneio.Encerrado)
            throw new InvalidOperationException("Torneios encerrados não podem ser cancelados");

        Status = StatusTorneio.Cancelado;
        Atualizar();
    }

    public void AdicionarAtleta(Atleta atleta)
    {
        ArgumentNullException.ThrowIfNull(atleta);

        if (Status != StatusTorneio.Planejado)
            throw new InvalidOperationException("Atletas só podem ser adicionados a torneios planejados");

        if (Categoria.HasValue && atleta.Categoria != Categoria.Value)
            throw new InvalidOperationException("Atleta não pertence à categoria do torneio");

        if (!_atletas.Contains(atleta))
        {
            _atletas.Add(atleta);
            Atualizar();
        }
    }

    public void RemoverAtleta(Atleta atleta)
    {
        ArgumentNullException.ThrowIfNull(atleta);

        if (Status != StatusTorneio.Planejado)
            throw new InvalidOperationException("Atletas só podem ser removidos de torneios planejados");

        _atletas.Remove(atleta);
        Atualizar();
    }
}

public class ConfiguracaoTorneio : ValueObject
{
    public int PontuacaoVitoria { get; private set; }
    public int PontuacaoDerrota { get; private set; }
    public int PontuacaoEmpate { get; private set; }
    public int PontuacaoWO { get; private set; }
    public bool PermiteEmpate { get; private set; }

    public ConfiguracaoTorneio()
    {
        PontuacaoVitoria = 10;
        PontuacaoDerrota = -5;
        PontuacaoEmpate = 0;
        PontuacaoWO = -20;
        PermiteEmpate = false;
    }

    public ConfiguracaoTorneio Atualizar(
        int? pontuacaoVitoria,
        int? pontuacaoDerrota,
        int? pontuacaoEmpate,
        int? pontuacaoWo,
        bool permiteEmpate)
    {
        var configuracao = new ConfiguracaoTorneio
        {
            PontuacaoVitoria = pontuacaoVitoria ?? PontuacaoVitoria,
            PontuacaoDerrota = pontuacaoDerrota ?? PontuacaoDerrota,
            PontuacaoEmpate = pontuacaoEmpate ?? PontuacaoEmpate,
            PontuacaoWO = pontuacaoWo ?? PontuacaoWO,
            PermiteEmpate = permiteEmpate
        };

        return configuracao;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PontuacaoVitoria;
        yield return PontuacaoDerrota;
        yield return PontuacaoEmpate;
        yield return PontuacaoWO;
        yield return PermiteEmpate;
    }
}
