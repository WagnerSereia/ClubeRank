using ClubeRank.Application.DTOs;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.ValueObjects;
using ClubeRank.Infrastructure.Data;
using ClubeRank.Infrastructure.Identity.Data;
using ClubeRank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityUserEntity = ClubeRank.Infrastructure.Identity.Entities.Usuario;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "organizacoes")]
public class OrganizacoesController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(OrganizacaoOnboardingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganizacaoOnboardingResponseDto>> Create(
        [FromBody] OnboardingOrganizacaoDto request,
        [FromServices] IOrganizacaoRepository organizacaoRepository,
        [FromServices] ClubeRankDbContext clubeRankDbContext,
        [FromServices] IdentityDbContext identityDbContext,
        [FromServices] UserManager<IdentityUserEntity> userManager,
        [FromServices] RoleManager<IdentityRole> roleManager)
    {
        ValidateRequest(request);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = new Email(request.Email);
        var existingOrganization = await organizacaoRepository
            .Listar();

        if (existingOrganization.Any(x => string.Equals(x.Email.Valor, email.Valor, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(request.Email), "Já existe uma organização cadastrada com este email.");
            return ValidationProblem(ModelState);
        }

        var existingUser = await userManager.FindByEmailAsync(email.Valor);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(request.Email), "Já existe um usuário administrador com este email.");
            return ValidationProblem(ModelState);
        }

        var organizacao = new Organizacao(
            request.Nome.Trim(),
            email,
            request.Telefone?.Trim(),
            request.Modalidade.Trim(),
            request.Plano);

        await organizacaoRepository.Adicionar(organizacao);

        try
        {
            await EnsureRoleExistsAsync(roleManager, PerfilUsuario.AdminOrganizacao.ToString());

            var clube = new Clube
            {
                Id = organizacao.Id,
                TenantId = organizacao.Id,
                Nome = organizacao.Nome
            };

            identityDbContext.Clubes.Add(clube);

            var adminUser = new IdentityUserEntity
            {
                UserName = email.Valor,
                Email = email.Valor,
                NomeCompleto = request.NomeAdministrador.Trim(),
                TenantId = organizacao.Id,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(adminUser, request.SenhaAdministrador);
            if (!createUserResult.Succeeded)
            {
                AddIdentityErrors(createUserResult);
                await RollbackOrganizationAsync(clubeRankDbContext, organizacao.Id);
                return ValidationProblem(ModelState);
            }

            var addRoleResult = await userManager.AddToRoleAsync(adminUser, PerfilUsuario.AdminOrganizacao.ToString());
            if (!addRoleResult.Succeeded)
            {
                AddIdentityErrors(addRoleResult);
                await userManager.DeleteAsync(adminUser);
                await RollbackOrganizationAsync(clubeRankDbContext, organizacao.Id);
                return ValidationProblem(ModelState);
            }

            identityDbContext.UsuariosClubes.Add(new UsuarioClube
            {
                UsuarioId = adminUser.Id,
                ClubeId = clube.Id,
                TenantId = organizacao.Id
            });

            await identityDbContext.SaveChangesAsync();

            var response = new OrganizacaoOnboardingResponseDto(
                organizacao.Id,
                organizacao.Id,
                organizacao.Nome,
                adminUser.Email ?? email.Valor,
                PerfilUsuario.AdminOrganizacao.ToString());

            return CreatedAtAction(nameof(GetById), new { id = organizacao.Id }, response);
        }
        catch
        {
            await RollbackOrganizationAsync(clubeRankDbContext, organizacao.Id);
            throw;
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)}")]
    [ProducesResponseType(typeof(OrganizacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizacaoDto>> GetById(
        Guid id,
        [FromServices] IOrganizacaoRepository organizacaoRepository)
    {
        var organizacao = await organizacaoRepository.ObterPorId(id);
        if (organizacao is null)
        {
            return NotFound();
        }

        return Ok(new OrganizacaoDto(
            organizacao.Id,
            organizacao.Nome,
            organizacao.Email.Valor,
            organizacao.Telefone,
            organizacao.Modalidade,
            organizacao.Status,
            organizacao.Plano,
            organizacao.LogoUrl,
            organizacao.DataCriacao));
    }

    private void ValidateRequest(OnboardingOrganizacaoDto request)
    {
        AddIfEmpty(nameof(request.Nome), request.Nome, "Nome da organização é obrigatório.");
        AddIfEmpty(nameof(request.Email), request.Email, "Email da organização é obrigatório.");
        AddIfEmpty(nameof(request.Modalidade), request.Modalidade, "Modalidade é obrigatória.");
        AddIfEmpty(nameof(request.NomeAdministrador), request.NomeAdministrador, "Nome do administrador é obrigatório.");
        AddIfEmpty(nameof(request.SenhaAdministrador), request.SenhaAdministrador, "Senha do administrador é obrigatória.");

        if (!string.IsNullOrWhiteSpace(request.SenhaAdministrador) && request.SenhaAdministrador.Length < 8)
        {
            ModelState.AddModelError(nameof(request.SenhaAdministrador), "A senha do administrador deve ter pelo menos 8 caracteres.");
        }
    }

    private void AddIfEmpty(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ModelState.AddModelError(key, message);
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task RollbackOrganizationAsync(ClubeRankDbContext clubeRankDbContext, Guid organizacaoId)
    {
        var organizacao = await clubeRankDbContext.Organizacoes.FirstOrDefaultAsync(x => x.Id == organizacaoId);
        if (organizacao is not null)
        {
            clubeRankDbContext.Organizacoes.Remove(organizacao);
            await clubeRankDbContext.SaveChangesAsync();
        }
    }
}
