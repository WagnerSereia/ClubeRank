using ClubeRank.Infrastructure.Identity.Data;
using ClubeRank.Infrastructure.Identity.Entities;
using ClubeRank.Infrastructure.Identity.MultiTenancy;
using ClubeRank.Infrastructure.Identity.Services;
using ClubeRank.Infrastructure.Identity.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ClubeRank.Infrastructure.Identity;

public static class DependencyInjection
{
    private const string IdentityConnectionStringName = "IdentityConnection";

    public static IServiceCollection AddInfrastructureIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(IdentityConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{IdentityConnectionStringName}' nao foi configurada.");
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Configuracao de JWT nao encontrada.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
            string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException("Configuracao de JWT invalida. Verifique Key, Issuer e Audience.");
        }

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddIdentityCore<Usuario>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
