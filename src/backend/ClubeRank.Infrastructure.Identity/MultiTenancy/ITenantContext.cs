namespace ClubeRank.Infrastructure.Identity.MultiTenancy;

public interface ITenantContext
{
    Guid? TenantId { get; set; }
}
