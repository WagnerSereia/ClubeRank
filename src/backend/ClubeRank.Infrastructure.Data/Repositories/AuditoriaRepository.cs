using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public AuditoriaRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Adicionar(Auditoria auditoria)
    {
        _dbContext.Auditorias.Add(auditoria);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Auditoria>> Listar(
        Guid? tenantId = null,
        Guid? usuarioId = null,
        string? entidade = null,
        string? acao = null,
        DateTime? dataInicialUtc = null,
        DateTime? dataFinalUtc = null)
    {
        IQueryable<Auditoria> query = _dbContext.Auditorias;

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

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .ToListAsync();
    }
}
