using ClubeRank.Infrastructure.Identity.MultiTenancy;
using System.Security.Claims;

namespace ClubeRank.Api.Middlewares;

public class TenantMiddleware
{
    public const string TenantHeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (IsIgnoredPath(context.Request.Path) || IsIgnoredEndpoint(context))
        {
            await _next(context);
            return;
        }

        if (!TryResolveTenantId(context, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(
                $"Informe o tenant pelo header '{TenantHeaderName}' ou use um bearer token com a claim 'tenant_id'.");
            return;
        }

        tenantContext.TenantId = tenantId;
        AddTenantClaimIfMissing(context, tenantId);
        await _next(context);
    }

    private static bool IsIgnoredPath(PathString path)
    {
        return path.StartsWithSegments("/swagger") ||
               path.StartsWithSegments("/health");
    }

    private static bool IsIgnoredEndpoint(HttpContext context)
    {
        return (HttpMethods.IsPost(context.Request.Method) &&
                context.Request.Path.StartsWithSegments("/api/organizacoes")) ||
               (HttpMethods.IsPost(context.Request.Method) &&
                context.Request.Path.StartsWithSegments("/api/auth/token"));
    }

    private static bool TryResolveTenantId(HttpContext context, out Guid tenantId)
    {
        tenantId = default;

        if (TryReadTenantFromHeader(context, out tenantId))
        {
            return true;
        }

        return TryReadTenantFromAuthenticatedUser(context, out tenantId);
    }

    private static bool TryReadTenantFromHeader(HttpContext context, out Guid tenantId)
    {
        tenantId = default;

        return context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeader) &&
               Guid.TryParse(tenantHeader, out tenantId);
    }

    private static bool TryReadTenantFromAuthenticatedUser(HttpContext context, out Guid tenantId)
    {
        tenantId = default;

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return Guid.TryParse(context.User.FindFirst("tenant_id")?.Value, out tenantId);
    }

    private static void AddTenantClaimIfMissing(HttpContext context, Guid tenantId)
    {
        if (context.User?.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return;
        }

        if (identity.HasClaim(claim => claim.Type == "tenant_id"))
        {
            return;
        }

        identity.AddClaim(new Claim("tenant_id", tenantId.ToString()));
    }
}
