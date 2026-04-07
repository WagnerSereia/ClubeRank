using System.Security.Claims;
using ClubeRank.Domain.Constants;

namespace ClubeRank.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetTenantId(this ClaimsPrincipal user, out Guid tenantId)
    {
        tenantId = default;
        return Guid.TryParse(user.FindFirst("tenant_id")?.Value, out tenantId);
    }

    public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        userId = default;
        return Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value, out userId);
    }

    public static Guid GetUserIdOrSystem(this ClaimsPrincipal user)
    {
        return user.TryGetUserId(out var userId)
            ? userId
            : SystemUsers.AuditFallbackUserId;
    }

    public static Guid? GetTenantIdOrNull(this ClaimsPrincipal user)
    {
        return user.TryGetTenantId(out var tenantId)
            ? tenantId
            : null;
    }

    public static Guid GetTenantIdOrGlobal(this ClaimsPrincipal user)
    {
        return user.TryGetTenantId(out var tenantId)
            ? tenantId
            : SystemTenants.GlobalTenantId;
    }
}
