using ClubeRank.Infrastructure.Identity.Entities;
using ClubeRank.Infrastructure.Identity.Mappings;
using ClubeRank.Infrastructure.Identity.MultiTenancy;
using ClubeRank.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Infrastructure.Identity.Data;

public class IdentityDbContext : IdentityDbContext<Usuario, IdentityRole, string>
{
    private readonly ITenantContext _tenantContext;
    private Guid CurrentTenantId => _tenantContext.TenantId ?? SystemTenants.GlobalTenantId;
    private bool HasTenant => _tenantContext.TenantId.HasValue;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Clube> Clubes => Set<Clube>();
    public DbSet<UsuarioClube> UsuariosClubes => Set<UsuarioClube>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UsuarioMap());
        builder.ApplyConfiguration(new ClubeMap());
        builder.ApplyConfiguration(new UsuarioClubeMap());

        builder.Entity<Usuario>().HasQueryFilter(x => HasTenant && x.TenantId == CurrentTenantId);
        builder.Entity<Clube>().HasQueryFilter(x => HasTenant && x.TenantId == CurrentTenantId);
        builder.Entity<UsuarioClube>().HasQueryFilter(x => HasTenant && x.TenantId == CurrentTenantId);
    }
}
