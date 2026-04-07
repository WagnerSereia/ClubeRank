using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class OrganizacaoRepository : IOrganizacaoRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public OrganizacaoRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Organizacao?> ObterPorId(Guid id)
    {
        return _dbContext.Organizacoes.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Adicionar(Organizacao organizacao)
    {
        _dbContext.Organizacoes.Add(organizacao);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Organizacao>> Listar()
    {
        return await _dbContext.Organizacoes
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }
}
