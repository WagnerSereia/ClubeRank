using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Interfaces;

public interface IAtletaRepository
{
    Task<Atleta?> ObterPorId(Guid id);
    Task Adicionar(Atleta atleta);
    Task Atualizar(Atleta atleta);
    Task<bool> ExisteEmailNaOrganizacao(Guid organizacaoId, string email, Guid? atletaIdIgnorar = null);
    Task<IEnumerable<Atleta>> ObterAtletasSemConfrontoHaDias(int dias);
    Task<IEnumerable<Atleta>> ObterRanking(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null);
    Task<IEnumerable<Atleta>> Listar(ListarAtletasDto filtros);
}

public interface IHistoricoRankingRepository
{
    Task Adicionar(HistoricoRanking historico);
    Task<IEnumerable<HistoricoRanking>> ObterPorAtletaId(Guid atletaId);
    Task<IReadOnlyCollection<Guid>> ObterAtletaIdsComHistoricoNoPeriodo(Guid tenantId, DateTime dataInicialUtc);
}

public interface IAuditoriaRepository
{
    Task Adicionar(Auditoria auditoria);
    Task<IEnumerable<Auditoria>> Listar(
        Guid? tenantId = null,
        Guid? usuarioId = null,
        string? entidade = null,
        string? acao = null,
        DateTime? dataInicialUtc = null,
        DateTime? dataFinalUtc = null);
}

public interface IOrganizacaoRepository
{
    Task<Organizacao?> ObterPorId(Guid id);
    Task Adicionar(Organizacao organizacao);
    Task<IEnumerable<Organizacao>> Listar();
}

public interface ITorneioRepository
{
    Task<Torneio?> ObterPorId(Guid id);
    Task<Torneio?> ObterPorIdComAtletas(Guid id);
    Task Adicionar(Torneio torneio);
    Task Atualizar(Torneio torneio);
    Task<IEnumerable<Torneio>> ListarPorOrganizacao(Guid organizacaoId);
}

public interface IConfrontoRepository
{
    Task<Confronto?> ObterPorId(Guid id);
    Task Adicionar(Confronto confronto);
    Task Atualizar(Confronto confronto);
    Task<IEnumerable<Confronto>> ListarRealizadosPorOrganizacao(Guid organizacaoId);
    Task<IEnumerable<Confronto>> ObterConfrontosEntreAtletasNoTorneio(Guid atletaAId, Guid atletaBId, Guid torneioId);
    Task<IEnumerable<Confronto>> ObterConfrontosDoAtletaNaData(Guid atletaId, DateTime data);
    Task<IEnumerable<Confronto>> ObterHistoricoConfrontosEntreAtletas(Guid atletaAId, Guid atletaBId, int limite);
    Task<IEnumerable<Confronto>> ListarPorTorneio(Guid torneioId);
}
