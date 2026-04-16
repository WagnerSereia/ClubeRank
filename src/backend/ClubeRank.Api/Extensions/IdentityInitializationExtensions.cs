using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;
using ClubeRank.Infrastructure.Data;
using ClubeRank.Infrastructure.Identity.Data;
using ClubeRank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Api.Extensions;

public static class IdentityInitializationExtensions
{
    private const string SeedOrganizationName = "Clube de Campo de Mogi das Cruzes";
    private const string SeedOrganizationEmail = "clube.wsinfo@msn.com";
    private const string SeedOrganizationModalidade = "Tenis";
    private const string SeedAdminName = "Wagner Sereia dos Santos";
    private const string SeedAdminEmail = "wsinfo@msn.com";
    private const string SeedAdminPassword = "W@gner1";

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

    public static async Task EnsureSeedDataAsync(IServiceProvider services)
    {
        var clubeRankDbContext = services.GetRequiredService<ClubeRankDbContext>();
        var identityDbContext = services.GetRequiredService<IdentityDbContext>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();

        var organizacao = await EnsureOrganizationAsync(clubeRankDbContext);
        await EnsureClubeAsync(identityDbContext, organizacao);
        var usuario = await EnsureUserAsync(userManager, identityDbContext, organizacao);
        await EnsureUserRolesAsync(userManager, usuario);
        await EnsureUserClubLinkAsync(identityDbContext, usuario, organizacao);
    }

    private static async Task<Organizacao> EnsureOrganizationAsync(ClubeRankDbContext clubeRankDbContext)
    {
        var email = new Email(SeedOrganizationEmail);
        var organizacao = await clubeRankDbContext.Organizacoes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email.Valor == email.Valor);

        if (organizacao is not null)
        {
            return organizacao;
        }

        organizacao = new Organizacao(
            SeedOrganizationName,
            email,
            null,
            SeedOrganizationModalidade,
            TipoPlano.Profissional);

        clubeRankDbContext.Organizacoes.Add(organizacao);
        await clubeRankDbContext.SaveChangesAsync();

        return organizacao;
    }

    private static async Task EnsureClubeAsync(IdentityDbContext identityDbContext, Organizacao organizacao)
    {
        var clube = await identityDbContext.Clubes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == organizacao.Id);

        if (clube is null)
        {
            identityDbContext.Clubes.Add(new Clube
            {
                Id = organizacao.Id,
                TenantId = organizacao.Id,
                Nome = organizacao.Nome
            });
        }
        else if (!string.Equals(clube.Nome, organizacao.Nome, StringComparison.Ordinal))
        {
            clube.Nome = organizacao.Nome;
        }

        await identityDbContext.SaveChangesAsync();
    }

    private static async Task<Usuario> EnsureUserAsync(
        UserManager<Usuario> userManager,
        IdentityDbContext identityDbContext,
        Organizacao organizacao)
    {
        var normalizedEmail = userManager.NormalizeEmail(SeedAdminEmail);

        var usuario = await identityDbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                UserName = SeedAdminEmail,
                Email = SeedAdminEmail,
                NomeCompleto = SeedAdminName,
                TenantId = organizacao.Id,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(usuario, SeedAdminPassword);
            EnsureIdentitySucceeded(createResult, "criar o usuario seed");

            return usuario;
        }

        usuario.UserName = SeedAdminEmail;
        usuario.Email = SeedAdminEmail;
        usuario.NomeCompleto = SeedAdminName;
        usuario.TenantId = organizacao.Id;
        usuario.EmailConfirmed = true;

        var updateResult = await userManager.UpdateAsync(usuario);
        EnsureIdentitySucceeded(updateResult, "atualizar o usuario seed");

        var passwordValid = await userManager.CheckPasswordAsync(usuario, SeedAdminPassword);
        if (!passwordValid)
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var resetResult = await userManager.ResetPasswordAsync(usuario, resetToken, SeedAdminPassword);
            EnsureIdentitySucceeded(resetResult, "definir a senha do usuario seed");
        }

        return usuario;
    }

    private static async Task EnsureUserRolesAsync(UserManager<Usuario> userManager, Usuario usuario)
    {
        var currentRoles = await userManager.GetRolesAsync(usuario);
        var missingRoles = Enum.GetNames<PerfilUsuario>()
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        var addRolesResult = await userManager.AddToRolesAsync(usuario, missingRoles);
        EnsureIdentitySucceeded(addRolesResult, "atribuir perfis ao usuario seed");
    }

    private static async Task EnsureUserClubLinkAsync(
        IdentityDbContext identityDbContext,
        Usuario usuario,
        Organizacao organizacao)
    {
        var userClubLinkExists = await identityDbContext.UsuariosClubes
            .IgnoreQueryFilters()
            .AnyAsync(x => x.UsuarioId == usuario.Id && x.ClubeId == organizacao.Id);

        if (userClubLinkExists)
        {
            return;
        }

        identityDbContext.UsuariosClubes.Add(new UsuarioClube
        {
            UsuarioId = usuario.Id,
            ClubeId = organizacao.Id,
            TenantId = organizacao.Id
        });

        await identityDbContext.SaveChangesAsync();
    }

    private static void EnsureIdentitySucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(x => $"{x.Code}: {x.Description}"));
        throw new InvalidOperationException($"Nao foi possivel {operation}. {errors}");
    }
}
