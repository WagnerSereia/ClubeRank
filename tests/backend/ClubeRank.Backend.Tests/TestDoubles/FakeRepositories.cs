using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Backend.Tests.TestDoubles;

internal sealed class FakeAtletaRepository : IAtletaRepository
{
    private readonly Dictionary<Guid, Atleta> _atletas = new();
    private readonly List<Atleta> _atletasInativos = new();

    public void Seed(params Atleta[] atletas)
    {
        foreach (var atleta in atletas)
        {
            _atletas[atleta.Id] = atleta;
        }
    }

    public void SeedAtletasInativos(params Atleta[] atletas)
    {
        _atletasInativos.Clear();
        _atletasInativos.AddRange(atletas);
        Seed(atletas);
    }

    public Task<Atleta?> ObterPorId(Guid id) =>
        Task.FromResult(_atletas.TryGetValue(id, out var atleta) ? atleta : null);

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
        var existe = _atletas.Values.Any(atleta =>
            atleta.OrganizacaoId == organizacaoId &&
            (atletaIdIgnorar is null || atleta.Id != atletaIdIgnorar.Value) &&
            string.Equals(atleta.Email.Valor, email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(existe);
    }

    public Task<IEnumerable<Atleta>> ObterAtletasSemConfrontoHaDias(int dias) =>
        Task.FromResult<IEnumerable<Atleta>>(_atletasInativos.ToArray());

    public Task<IEnumerable<Atleta>> ObterRanking(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null)
    {
        var query = _atletas.Values.Where(x => x.OrganizacaoId == organizacaoId);

        if (categoria.HasValue)
        {
            query = query.Where(x => x.Categoria == categoria.Value);
        }

        if (genero.HasValue)
        {
            query = query.Where(x => x.Genero == genero.Value);
        }

        return Task.FromResult<IEnumerable<Atleta>>(query.OrderByDescending(x => x.PontuacaoAtual.Valor).ToArray());
    }

    public Task<IEnumerable<Atleta>> Listar(ListarAtletasDto filtros)
    {
        IEnumerable<Atleta> query = _atletas.Values;
        if (filtros.FiltroStatus.HasValue)
        {
            query = query.Where(x => x.Status == filtros.FiltroStatus.Value);
        }

        if (filtros.FiltroCategoria.HasValue)
        {
            query = query.Where(x => x.Categoria == filtros.FiltroCategoria.Value);
        }

        if (filtros.FiltroGenero.HasValue)
        {
            query = query.Where(x => x.Genero == filtros.FiltroGenero.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.FiltroNome))
        {
            query = query.Where(x => x.Nome.NomeFormatado.Contains(filtros.FiltroNome, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<Atleta>>(query.ToArray());
    }
}

internal sealed class FakeHistoricoRankingRepository : IHistoricoRankingRepository
{
    private readonly List<HistoricoRanking> _historicos = new();

    public IReadOnlyCollection<HistoricoRanking> Historicos => _historicos;

    public Task Adicionar(HistoricoRanking historico)
    {
        _historicos.Add(historico);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<HistoricoRanking>> ObterPorAtletaId(Guid atletaId) =>
        Task.FromResult<IEnumerable<HistoricoRanking>>(_historicos.Where(x => x.AtletaId == atletaId).ToArray());

    public Task<IReadOnlyCollection<Guid>> ObterAtletaIdsComHistoricoNoPeriodo(Guid tenantId, DateTime dataInicialUtc)
    {
        IReadOnlyCollection<Guid> atletaIds = _historicos
            .Where(x => x.TenantId == tenantId && x.DataCriacao >= dataInicialUtc)
            .Select(x => x.AtletaId)
            .Distinct()
            .ToArray();

        return Task.FromResult(atletaIds);
    }
}

internal sealed class FakeOrganizacaoRepository : IOrganizacaoRepository
{
    private readonly Dictionary<Guid, Organizacao> _organizacoes = new();

    public void Seed(params Organizacao[] organizacoes)
    {
        foreach (var organizacao in organizacoes)
        {
            _organizacoes[organizacao.Id] = organizacao;
        }
    }

    public Task<Organizacao?> ObterPorId(Guid id) =>
        Task.FromResult(_organizacoes.TryGetValue(id, out var organizacao) ? organizacao : null);

    public Task Adicionar(Organizacao organizacao)
    {
        _organizacoes[organizacao.Id] = organizacao;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Organizacao>> Listar() =>
        Task.FromResult<IEnumerable<Organizacao>>(_organizacoes.Values.ToArray());
}

internal sealed class FakeTorneioRepository : ITorneioRepository
{
    private readonly Dictionary<Guid, Torneio> _torneios = new();

    public Task<Torneio?> ObterPorId(Guid id) =>
        Task.FromResult(_torneios.TryGetValue(id, out var torneio) ? torneio : null);

    public Task<Torneio?> ObterPorIdComAtletas(Guid id) =>
        Task.FromResult(_torneios.TryGetValue(id, out var torneio) ? torneio : null);

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

    public Task<IEnumerable<Torneio>> ListarPorOrganizacao(Guid organizacaoId) =>
        Task.FromResult<IEnumerable<Torneio>>(_torneios.Values.Where(x => x.OrganizacaoId == organizacaoId).ToArray());
}

internal sealed class FakeConfrontoRepository : IConfrontoRepository
{
    private readonly Dictionary<Guid, Confronto> _confrontos = new();

    public IReadOnlyCollection<Confronto> Confrontos => _confrontos.Values.ToArray();

    public Task<Confronto?> ObterPorId(Guid id) =>
        Task.FromResult(_confrontos.TryGetValue(id, out var confronto) ? confronto : null);

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

    public Task<IEnumerable<Confronto>> ListarRealizadosPorOrganizacao(Guid organizacaoId)
    {
        var confrontos = _confrontos.Values
            .Where(x => x.TenantId == organizacaoId && x.Status == StatusConfronto.Realizado)
            .ToArray();

        return Task.FromResult<IEnumerable<Confronto>>(confrontos);
    }

    public Task<IEnumerable<Confronto>> ObterConfrontosEntreAtletasNoTorneio(Guid atletaAId, Guid atletaBId, Guid torneioId)
    {
        var confrontos = _confrontos.Values.Where(x =>
            x.TorneioId == torneioId &&
            ((x.AtletaAId == atletaAId && x.AtletaBId == atletaBId) ||
             (x.AtletaAId == atletaBId && x.AtletaBId == atletaAId)));

        return Task.FromResult<IEnumerable<Confronto>>(confrontos.ToArray());
    }

    public Task<IEnumerable<Confronto>> ObterConfrontosDoAtletaNaData(Guid atletaId, DateTime data)
    {
        var confrontos = _confrontos.Values.Where(x =>
            x.DataAgendamento.Date == data.Date &&
            (x.AtletaAId == atletaId || x.AtletaBId == atletaId));

        return Task.FromResult<IEnumerable<Confronto>>(confrontos.ToArray());
    }

    public Task<IEnumerable<Confronto>> ObterHistoricoConfrontosEntreAtletas(Guid atletaAId, Guid atletaBId, int limite)
    {
        var confrontos = _confrontos.Values.Where(x =>
            (x.AtletaAId == atletaAId && x.AtletaBId == atletaBId) ||
            (x.AtletaAId == atletaBId && x.AtletaBId == atletaAId));

        return Task.FromResult<IEnumerable<Confronto>>(confrontos.Take(limite).ToArray());
    }

    public Task<IEnumerable<Confronto>> ListarPorTorneio(Guid torneioId)
    {
        var confrontos = _confrontos.Values.Where(x => x.TorneioId == torneioId);
        return Task.FromResult<IEnumerable<Confronto>>(confrontos.ToArray());
    }
}
