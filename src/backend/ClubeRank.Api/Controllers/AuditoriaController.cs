using ClubeRank.Api.Extensions;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.Auditor)}")]
public class AuditoriaController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AuditoriaDto>>> List(
        [FromServices] IAuditoriaRepository auditoriaRepository,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] string? entidade = null,
        [FromQuery] string? acao = null,
        [FromQuery] DateTime? dataInicialUtc = null,
        [FromQuery] DateTime? dataFinalUtc = null)
    {
        if (!User.TryGetTenantId(out var tenantId) && !User.IsInRole(nameof(PerfilUsuario.SuperAdmin)))
        {
            return BadRequest("Tenant do usuario autenticado nao encontrado.");
        }

        var auditorias = await auditoriaRepository.Listar(
            User.IsInRole(nameof(PerfilUsuario.SuperAdmin)) ? null : tenantId,
            usuarioId,
            entidade,
            acao,
            dataInicialUtc,
            dataFinalUtc);

        return Ok(auditorias.Select(x => new AuditoriaDto(
            x.Id,
            x.OrganizacaoId,
            x.UsuarioId,
            x.Entidade,
            x.EntidadeId,
            x.Acao,
            x.DadosAntes,
            x.DadosDepois,
            x.Ip,
            x.DataCriacao)));
    }

    public record AuditoriaDto(
        Guid Id,
        Guid? OrganizacaoId,
        Guid? UsuarioId,
        string Entidade,
        string EntidadeId,
        string Acao,
        string? DadosAntes,
        string? DadosDepois,
        string? Ip,
        DateTime DataAcao);
}
