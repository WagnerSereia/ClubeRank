using ClubeRank.Infrastructure.Data;
using ClubeRank.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubeRank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuracoes comuns da infraestrutura e registro dos modulos.
        services.AddInfrastructureIdentity(configuration);
        services.AddInfrastructureData(configuration);

        return services;
    }
}
