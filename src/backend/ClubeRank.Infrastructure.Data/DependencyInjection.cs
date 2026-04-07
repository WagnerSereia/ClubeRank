using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Infrastructure.Identity.MultiTenancy;
using ClubeRank.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubeRank.Infrastructure.Data;

public static class DependencyInjection
{
    private const string AppConnectionStringName = "ApplicationConnection";
    private const string FallbackConnectionStringName = "IdentityConnection";

    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(AppConnectionStringName) ??
            configuration.GetConnectionString(FallbackConnectionStringName) ??
            throw new InvalidOperationException(
                $"Connection string '{AppConnectionStringName}' ou '{FallbackConnectionStringName}' nao foi configurada.");

        services.AddDbContext<ClubeRankDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IAtletaRepository, AtletaRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddScoped<IHistoricoRankingRepository, HistoricoRankingRepository>();
        services.AddScoped<IOrganizacaoRepository, OrganizacaoRepository>();
        services.AddScoped<ITorneioRepository, TorneioRepository>();
        services.AddScoped<IConfrontoRepository, ConfrontoRepository>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IConfrontoService, ConfrontoService>();

        return services;
    }
}
