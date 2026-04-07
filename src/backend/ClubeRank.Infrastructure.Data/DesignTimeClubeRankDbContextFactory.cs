using ClubeRank.Infrastructure.Identity.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClubeRank.Infrastructure.Data;

public class DesignTimeClubeRankDbContextFactory : IDesignTimeDbContextFactory<ClubeRankDbContext>
{
    public ClubeRankDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ClubeRank.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("ApplicationConnection") ??
            configuration.GetConnectionString("IdentityConnection") ??
            throw new InvalidOperationException("Nenhuma connection string configurada para o ClubeRankDbContext.");

        var optionsBuilder = new DbContextOptionsBuilder<ClubeRankDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ClubeRankDbContext(optionsBuilder.Options, new TenantContext());
    }
}
