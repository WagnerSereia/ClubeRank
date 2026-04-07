using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class HistoricoRankingRepository : IHistoricoRankingRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public HistoricoRankingRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Adicionar(HistoricoRanking historico)
    {
        _dbContext.HistoricosRanking.Add(historico);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<HistoricoRanking>> ObterPorAtletaId(Guid atletaId)
    {
        return await _dbContext.HistoricosRanking
            .Include(x => x.Confronto)
                .ThenInclude(x => x!.AtletaA)
            .Include(x => x.Confronto)
                .ThenInclude(x => x!.AtletaB)
            .Where(x => x.AtletaId == atletaId)
            .OrderByDescending(x => x.DataCriacao)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Guid>> ObterAtletaIdsComHistoricoNoPeriodo(Guid tenantId, DateTime dataInicialUtc)
    {
        return await _dbContext.HistoricosRanking
            .Where(x => x.TenantId == tenantId && x.DataCriacao >= dataInicialUtc)
            .Select(x => x.AtletaId)
            .Distinct()
            .ToListAsync();
    }
}
