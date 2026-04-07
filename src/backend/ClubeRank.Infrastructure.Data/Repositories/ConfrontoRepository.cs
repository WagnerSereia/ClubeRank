using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class ConfrontoRepository : IConfrontoRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public ConfrontoRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Confronto?> ObterPorId(Guid id)
    {
        return _dbContext.Confrontos
            .Include(x => x.AtletaA)
            .Include(x => x.AtletaB)
            .Include(x => x.Torneio)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Adicionar(Confronto confronto)
    {
        _dbContext.Confrontos.Add(confronto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Atualizar(Confronto confronto)
    {
        _dbContext.Confrontos.Update(confronto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Confronto>> ObterConfrontosEntreAtletasNoTorneio(Guid atletaAId, Guid atletaBId, Guid torneioId)
    {
        return await _dbContext.Confrontos
            .Where(x =>
                x.TorneioId == torneioId &&
                ((x.AtletaAId == atletaAId && x.AtletaBId == atletaBId) ||
                 (x.AtletaAId == atletaBId && x.AtletaBId == atletaAId)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Confronto>> ObterConfrontosDoAtletaNaData(Guid atletaId, DateTime data)
    {
        return await _dbContext.Confrontos
            .Where(x =>
                x.DataAgendamento.Date == data.Date &&
                (x.AtletaAId == atletaId || x.AtletaBId == atletaId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Confronto>> ObterHistoricoConfrontosEntreAtletas(Guid atletaAId, Guid atletaBId, int limite)
    {
        return await _dbContext.Confrontos
            .Where(x =>
                ((x.AtletaAId == atletaAId && x.AtletaBId == atletaBId) ||
                 (x.AtletaAId == atletaBId && x.AtletaBId == atletaAId)) &&
                x.Status == StatusConfronto.Realizado)
            .OrderByDescending(x => x.DataAgendamento)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<IEnumerable<Confronto>> ListarPorTorneio(Guid torneioId)
    {
        return await _dbContext.Confrontos
            .Include(x => x.AtletaA)
            .Include(x => x.AtletaB)
            .Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.DataAgendamento)
            .ToListAsync();
    }
}
