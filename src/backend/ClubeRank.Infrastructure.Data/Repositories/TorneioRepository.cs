using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class TorneioRepository : ITorneioRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public TorneioRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Torneio?> ObterPorId(Guid id)
    {
        return _dbContext.Torneios.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Torneio?> ObterPorIdComAtletas(Guid id)
    {
        return _dbContext.Torneios
            .Include(x => x.Atletas)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Adicionar(Torneio torneio)
    {
        _dbContext.Torneios.Add(torneio);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Atualizar(Torneio torneio)
    {
        _dbContext.Torneios.Update(torneio);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Torneio>> ListarPorOrganizacao(Guid organizacaoId)
    {
        return await _dbContext.Torneios
            .Include(x => x.Atletas)
            .Where(x => x.OrganizacaoId == organizacaoId)
            .OrderByDescending(x => x.DataCriacao)
            .ToListAsync();
    }
}
