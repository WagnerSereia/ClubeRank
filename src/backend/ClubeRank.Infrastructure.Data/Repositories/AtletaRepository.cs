using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data.Repositories;

public class AtletaRepository : IAtletaRepository
{
    private readonly ClubeRankDbContext _dbContext;

    public AtletaRepository(ClubeRankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Atleta?> ObterPorId(Guid id)
    {
        return _dbContext.Atletas.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Adicionar(Atleta atleta)
    {
        _dbContext.Atletas.Add(atleta);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Atualizar(Atleta atleta)
    {
        _dbContext.Atletas.Update(atleta);
        await _dbContext.SaveChangesAsync();
    }

    public Task<bool> ExisteEmailNaOrganizacao(Guid organizacaoId, string email, Guid? atletaIdIgnorar = null)
    {
        var emailNormalizado = email.Trim().ToUpperInvariant();

        return _dbContext.Atletas.AnyAsync(x =>
            x.OrganizacaoId == organizacaoId &&
            (atletaIdIgnorar == null || x.Id != atletaIdIgnorar.Value) &&
            x.Email.Valor.ToUpper() == emailNormalizado);
    }

    public async Task<IEnumerable<Atleta>> ObterAtletasSemConfrontoHaDias(int dias)
    {
        var limite = DateTime.UtcNow.AddDays(-dias);

        var atletas = await _dbContext.Atletas
            .Where(atleta =>
                atleta.DataCriacao <= limite &&
                !_dbContext.Confrontos.Any(confronto =>
                    (confronto.AtletaAId == atleta.Id || confronto.AtletaBId == atleta.Id) &&
                    confronto.Status == StatusConfronto.Realizado &&
                    confronto.DataAgendamento >= limite))
            .ToListAsync();

        return atletas;
    }

    public async Task<IEnumerable<Atleta>> ObterRanking(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null)
    {
        IQueryable<Atleta> query = _dbContext.Atletas.Where(x => x.OrganizacaoId == organizacaoId);

        if (categoria.HasValue)
        {
            query = query.Where(x => x.Categoria == categoria.Value);
        }

        if (genero.HasValue)
        {
            query = query.Where(x => x.Genero == genero.Value);
        }

        return await query
            .OrderByDescending(x => x.PontuacaoAtual.Valor)
            .ThenBy(x => x.Nome.PrimeiroNome)
            .ThenBy(x => x.Nome.Sobrenome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Atleta>> Listar(ListarAtletasDto filtros)
    {
        IQueryable<Atleta> query = _dbContext.Atletas;

        if (!string.IsNullOrWhiteSpace(filtros.FiltroNome))
        {
            var nome = filtros.FiltroNome.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.Nome.PrimeiroNome, $"%{nome}%") ||
                EF.Functions.Like(x.Nome.Sobrenome, $"%{nome}%"));
        }

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

        query = filtros.Ordenacao?.ToLowerInvariant() switch
        {
            "ranking" => query.OrderByDescending(x => x.PontuacaoAtual.Valor),
            "data" => query.OrderByDescending(x => x.DataCriacao),
            _ => query.OrderBy(x => x.Nome.PrimeiroNome).ThenBy(x => x.Nome.Sobrenome)
        };

        var pagina = Math.Max(1, filtros.Pagina);
        var tamanhoPagina = Math.Max(1, filtros.TamanhoPagina);

        return await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }
}
