using ClubeRank.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ClubeRank.Api.Extensions;

public static class IdentityInitializationExtensions
{
    public static async Task EnsureRolesCreatedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Enum.GetNames<PerfilUsuario>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
