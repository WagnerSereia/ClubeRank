namespace ClubeRank.Infrastructure.Identity.MultiTenancy;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}
