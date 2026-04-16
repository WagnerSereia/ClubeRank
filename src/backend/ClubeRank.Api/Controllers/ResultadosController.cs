using ClubeRank.Api.Extensions;
using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "competicoes")]
[Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorConfrontos)}")]
public class ResultadosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
        [FromBody] RegisterResultadoRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return BadRequest("Usuario autenticado nao encontrado.");
        }

        try
        {
            await mediator.Send(new RegistrarResultadoCommand(new RegistrarResultadoDto(
                request.ConfrontoId,
                request.TipoResultado,
                request.Sets?.Select(x => new SetResultadoDto(x.Numero, x.GamesAtletaA, x.GamesAtletaB, x.TieBreak)).ToArray(),
                request.JustificativaWO?.Trim(),
                userId)));
            var tenantId = User.GetTenantIdOrNull();
            await auditoriaService.RegistrarAsync(
                tenantId,
                userId,
                "Resultado",
                request.ConfrontoId.ToString(),
                "Registro",
                null,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError("resultado", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    public record RegisterResultadoRequest(
        Guid ConfrontoId,
        TipoResultado? TipoResultado,
        IReadOnlyCollection<SetRequest>? Sets,
        string? JustificativaWO);

    public record SetRequest(
        int Numero,
        int GamesAtletaA,
        int GamesAtletaB,
        bool TieBreak = false);
}
