using Microsoft.Extensions.DependencyInjection;

namespace ClubeRank.Infrastructure.CrossCutting;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureCrossCutting(this IServiceCollection services)
    {
        // Configurações de DI para CrossCutting
        // Logging, etc.

        return services;
    }
}