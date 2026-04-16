using ClubeRank.Api.Extensions;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.Queries;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "competicoes")]
[Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)},{nameof(PerfilUsuario.Auditor)}")]
public class RankingsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AtletaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AtletaDto>>> Get(
        [FromServices] IMediator mediator,
        [FromQuery] Categoria? categoria = null,
        [FromQuery] Genero? genero = null,
        [FromQuery] int? periodoDias = null)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return BadRequest("Tenant do usuário autenticado não encontrado.");
        }

        var ranking = await mediator.Send(new ObterRankingQuery(tenantId, categoria, genero, periodoDias));
        return Ok(ranking);
    }

    [HttpPost("decaimento")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.Auditor)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ApplyDecay(
        [FromServices] IRankingService rankingService,
        [FromServices] IAuditoriaService auditoriaService)
    {
        await rankingService.AplicarDecaimentoAtletasInativos();
        var tenantId = User.GetTenantIdOrGlobal();
        var userId = User.GetUserIdOrSystem();
        await auditoriaService.RegistrarAsync(
            tenantId,
            userId,
            "Ranking",
            tenantId.ToString(),
            "Decaimento",
            null,
            new { mensagem = "Decaimento por inatividade aplicado" },
            HttpContext.Connection.RemoteIpAddress?.ToString());
        return NoContent();
    }
}
