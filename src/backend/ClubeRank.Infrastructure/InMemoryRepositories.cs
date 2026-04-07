using System.Collections.Concurrent;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace ClubeRank.Infrastructure;

internal sealed class InMemoryAtletaRepository : IAtletaRepository
{
    private readonly ConcurrentDictionary<Guid, Atleta> _atletas = new();
    private readonly IConfrontoRepository _confrontoRepository;

    public InMemoryAtletaRepository(IConfrontoRepository confrontoRepository)
    {
        _confrontoRepository = confrontoRepository;
    }

    public Task<Atleta?> ObterPorId(Guid id)
    {
        _atletas.TryGetValue(id, out var atleta);
        return Task.FromResult(atleta);
    }

    public Task Adicionar(Atleta atleta)
    {
        _atletas[atleta.Id] = atleta;
        return Task.CompletedTask;
    }

    public Task Atualizar(Atleta atleta)
    {
        _atletas[atleta.Id] = atleta;
        return Task.CompletedTask;
    }

    public Task<bool> ExisteEmailNaOrganizacao(Guid organizacaoId, string email, Guid? atletaIdIgnorar = null)
    {
        var emailNormalizado = email.Trim();
        var existe = _atletas.Values.Any(atleta =>
            atleta.OrganizacaoId == organizacaoId &&
            (atletaIdIgnorar == null || atleta.Id != atletaIdIgnorar.Value) &&
            string.Equals(atleta.Email.Valor, emailNormalizado, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(existe);
    }

    public Task<IEnumerable<Atleta>> ObterAtletasSemConfrontoHaDias(int dias)
    {
        var limite = DateTime.UtcNow.AddDays(-dias);
        var atletas = _atletas.Values
            .Where(atleta => atleta.PontuacaoAtual.DataAtualizacao <= limite)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(atletas);
    }

    public Task<IEnumerable<Atleta>> ObterRanking(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null)
    {
        var query = _atletas.Values.Where(atleta => atleta.OrganizacaoId == organizacaoId);

        if (categoria.HasValue)
        {
            query = query.Where(atleta => atleta.Categoria == categoria.Value);
        }

        if (genero.HasValue)
        {
            query = query.Where(atleta => atleta.Genero == genero.Value);
        }

        var ranking = query
            .OrderByDescending(atleta => atleta.PontuacaoAtual.Valor)
            .ThenBy(atleta => atleta.Nome.NomeFormatado)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(ranking);
    }

    public Task<IEnumerable<Atleta>> Listar(ListarAtletasDto filtros)
    {
        var query = _atletas.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtros.FiltroNome))
        {
            query = query.Where(atleta =>
                atleta.Nome.NomeFormatado.Contains(filtros.FiltroNome, StringComparison.OrdinalIgnoreCase));
        }

        if (filtros.FiltroStatus.HasValue)
        {
            query = query.Where(atleta => atleta.Status == filtros.FiltroStatus.Value);
        }

        if (filtros.FiltroCategoria.HasValue)
        {
            query = query.Where(atleta => atleta.Categoria == filtros.FiltroCategoria.Value);
        }

        if (filtros.FiltroGenero.HasValue)
        {
            query = query.Where(atleta => atleta.Genero == filtros.FiltroGenero.Value);
        }

        query = filtros.Ordenacao?.ToLowerInvariant() switch
        {
            "ranking" => query.OrderByDescending(atleta => atleta.PontuacaoAtual.Valor),
            "data" => query.OrderByDescending(atleta => atleta.DataCriacao),
            _ => query.OrderBy(atleta => atleta.Nome.NomeFormatado)
        };

        var pagina = Math.Max(1, filtros.Pagina);
        var tamanhoPagina = Math.Max(1, filtros.TamanhoPagina);

        var atletas = query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(atletas);
    }
}

internal sealed class InMemoryHistoricoRankingRepository : IHistoricoRankingRepository
{
    private readonly ConcurrentDictionary<Guid, HistoricoRanking> _historicos = new();

    public Task Adicionar(HistoricoRanking historico)
    {
        _historicos[historico.Id] = historico;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<HistoricoRanking>> ObterPorAtletaId(Guid atletaId)
    {
        var historicos = _historicos.Values
            .Where(item => item.AtletaId == atletaId)
            .OrderByDescending(item => item.DataCriacao)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(historicos);
    }

    public Task<IReadOnlyCollection<Guid>> ObterAtletaIdsComHistoricoNoPeriodo(Guid tenantId, DateTime dataInicialUtc)
    {
        IReadOnlyCollection<Guid> atletaIds = _historicos.Values
            .Where(item => item.TenantId == tenantId && item.DataCriacao >= dataInicialUtc)
            .Select(item => item.AtletaId)
            .Distinct()
            .ToArray();

        return Task.FromResult(atletaIds);
    }
}

internal sealed class InMemoryAuditoriaRepository : IAuditoriaRepository
{
    private readonly ConcurrentDictionary<Guid, Auditoria> _auditorias = new();

    public Task Adicionar(Auditoria auditoria)
    {
        _auditorias[auditoria.Id] = auditoria;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Auditoria>> Listar(
        Guid? tenantId = null,
        Guid? usuarioId = null,
        string? entidade = null,
        string? acao = null,
        DateTime? dataInicialUtc = null,
        DateTime? dataFinalUtc = null)
    {
        var query = _auditorias.Values.AsEnumerable();

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.OrganizacaoId == tenantId.Value);
        }

        if (usuarioId.HasValue)
        {
            query = query.Where(x => x.UsuarioId == usuarioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entidade))
        {
            query = query.Where(x => x.Entidade == entidade);
        }

        if (!string.IsNullOrWhiteSpace(acao))
        {
            query = query.Where(x => x.Acao == acao);
        }

        if (dataInicialUtc.HasValue)
        {
            query = query.Where(x => x.DataCriacao >= dataInicialUtc.Value);
        }

        if (dataFinalUtc.HasValue)
        {
            query = query.Where(x => x.DataCriacao <= dataFinalUtc.Value);
        }

        return Task.FromResult<IEnumerable<Auditoria>>(query.OrderByDescending(x => x.DataCriacao).ToArray());
    }
}

internal sealed class InMemoryOrganizacaoRepository : IOrganizacaoRepository
{
    private readonly ConcurrentDictionary<Guid, Organizacao> _organizacoes = new();

    public Task<Organizacao?> ObterPorId(Guid id)
    {
        _organizacoes.TryGetValue(id, out var organizacao);
        return Task.FromResult(organizacao);
    }

    public Task Adicionar(Organizacao organizacao)
    {
        _organizacoes[organizacao.Id] = organizacao;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Organizacao>> Listar()
    {
        return Task.FromResult(_organizacoes.Values.OrderBy(x => x.Nome).AsEnumerable());
    }
}

internal sealed class InMemoryTorneioRepository : ITorneioRepository
{
    private readonly ConcurrentDictionary<Guid, Torneio> _torneios = new();

    public Task<Torneio?> ObterPorId(Guid id)
    {
        _torneios.TryGetValue(id, out var torneio);
        return Task.FromResult(torneio);
    }

    public Task<Torneio?> ObterPorIdComAtletas(Guid id)
    {
        _torneios.TryGetValue(id, out var torneio);
        return Task.FromResult(torneio);
    }

    public Task Adicionar(Torneio torneio)
    {
        _torneios[torneio.Id] = torneio;
        return Task.CompletedTask;
    }

    public Task Atualizar(Torneio torneio)
    {
        _torneios[torneio.Id] = torneio;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Torneio>> ListarPorOrganizacao(Guid organizacaoId)
    {
        var torneios = _torneios.Values
            .Where(item => item.OrganizacaoId == organizacaoId)
            .OrderByDescending(item => item.DataCriacao)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(torneios);
    }
}

internal sealed class InMemoryConfrontoRepository : IConfrontoRepository
{
    private readonly ConcurrentDictionary<Guid, Confronto> _confrontos = new();

    public Task<Confronto?> ObterPorId(Guid id)
    {
        _confrontos.TryGetValue(id, out var confronto);
        return Task.FromResult(confronto);
    }

    public Task Adicionar(Confronto confronto)
    {
        _confrontos[confronto.Id] = confronto;
        return Task.CompletedTask;
    }

    public Task Atualizar(Confronto confronto)
    {
        _confrontos[confronto.Id] = confronto;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Confronto>> ObterConfrontosEntreAtletasNoTorneio(Guid atletaAId, Guid atletaBId, Guid torneioId)
    {
        var confrontos = _confrontos.Values
            .Where(confronto =>
                confronto.TorneioId == torneioId &&
                ((confronto.AtletaAId == atletaAId && confronto.AtletaBId == atletaBId) ||
                 (confronto.AtletaAId == atletaBId && confronto.AtletaBId == atletaAId)))
            .OrderByDescending(confronto => confronto.DataAgendamento)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(confrontos);
    }

    public Task<IEnumerable<Confronto>> ObterConfrontosDoAtletaNaData(Guid atletaId, DateTime data)
    {
        var confrontos = _confrontos.Values
            .Where(confronto =>
                confronto.DataAgendamento.Date == data.Date &&
                (confronto.AtletaAId == atletaId || confronto.AtletaBId == atletaId))
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(confrontos);
    }

    public Task<IEnumerable<Confronto>> ObterHistoricoConfrontosEntreAtletas(Guid atletaAId, Guid atletaBId, int limite)
    {
        var confrontos = _confrontos.Values
            .Where(confronto =>
                (confronto.AtletaAId == atletaAId && confronto.AtletaBId == atletaBId) ||
                (confronto.AtletaAId == atletaBId && confronto.AtletaBId == atletaAId))
            .OrderByDescending(confronto => confronto.DataAgendamento)
            .Take(limite)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(confrontos);
    }

    public Task<IEnumerable<Confronto>> ListarPorTorneio(Guid torneioId)
    {
        var confrontos = _confrontos.Values
            .Where(confronto => confronto.TorneioId == torneioId)
            .OrderBy(confronto => confronto.DataAgendamento)
            .ToArray()
            .AsEnumerable();

        return Task.FromResult(confrontos);
    }
}

internal static class InMemoryInfrastructureRegistration
{
    public static IServiceCollection AddInMemoryClubeRankCore(this IServiceCollection services)
    {
        services.AddSingleton<IConfrontoRepository, InMemoryConfrontoRepository>();
        services.AddSingleton<IAtletaRepository, InMemoryAtletaRepository>();
        services.AddSingleton<IAuditoriaRepository, InMemoryAuditoriaRepository>();
        services.AddSingleton<IHistoricoRankingRepository, InMemoryHistoricoRankingRepository>();
        services.AddSingleton<IOrganizacaoRepository, InMemoryOrganizacaoRepository>();
        services.AddSingleton<ITorneioRepository, InMemoryTorneioRepository>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IConfrontoService, ConfrontoService>();

        return services;
    }
}
