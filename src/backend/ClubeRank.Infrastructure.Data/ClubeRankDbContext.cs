using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Constants;
using ClubeRank.Infrastructure.Identity.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Data;

public class ClubeRankDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private Guid CurrentTenantId => _tenantContext.TenantId ?? SystemTenants.GlobalTenantId;
    private bool HasTenant => _tenantContext.TenantId.HasValue;

    public ClubeRankDbContext(
        DbContextOptions<ClubeRankDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Atleta> Atletas => Set<Atleta>();
    public DbSet<Torneio> Torneios => Set<Torneio>();
    public DbSet<Confronto> Confrontos => Set<Confronto>();
    public DbSet<HistoricoRanking> HistoricosRanking => Set<HistoricoRanking>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClubeRankDbContext).Assembly);
        modelBuilder.Entity<Organizacao>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
        modelBuilder.Entity<Atleta>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
        modelBuilder.Entity<Torneio>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
        modelBuilder.Entity<Confronto>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
        modelBuilder.Entity<HistoricoRanking>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
        modelBuilder.Entity<Auditoria>()
            .HasQueryFilter(x => !HasTenant || x.TenantId == CurrentTenantId);
    }
}
