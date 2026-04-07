using ClubeRank.Api.Extensions;
using ClubeRank.Application.Commands;
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
[Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorConfrontos)},{nameof(PerfilUsuario.Auditor)}")]
public class TorneiosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TorneioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TorneioDto>>> List(
        [FromServices] IMediator mediator)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return BadRequest("Tenant do usuário autenticado não encontrado.");
        }

        var torneios = await mediator.Send(new ListarTorneiosOrganizacaoQuery(tenantId));
        return Ok(torneios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TorneioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TorneioDto>> GetById(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var torneio = await mediator.Send(new ObterTorneioPorIdQuery(id));
        if (torneio is null)
        {
            return NotFound();
        }

        return Ok(torneio);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorConfrontos)}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(
        [FromBody] CreateTorneioRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return BadRequest("Tenant do usuário autenticado não encontrado.");
        }

        try
        {
            var torneioId = await mediator.Send(new CriarTorneioCommand(new CriarTorneioDto(
                request.Nome.Trim(),
                request.Descricao?.Trim(),
                request.Tipo,
                tenantId,
                request.Categoria,
                request.DataInicio,
                request.DataFim,
                request.PontuacaoVitoria,
                request.PontuacaoDerrota,
                request.PontuacaoEmpate,
                request.PontuacaoWO,
                request.PermiteEmpate)));
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Torneio",
                torneioId.ToString(),
                "Criacao",
                null,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetById), new { id = torneioId }, new { id = torneioId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("torneio", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("{id:guid}/atletas")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorConfrontos)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddAthlete(
        Guid id,
        [FromBody] AddAtletaTorneioRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            await mediator.Send(new AdicionarAtletaTorneioCommand(new AdicionarAtletaTorneioDto(id, request.AtletaId)));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Torneio",
                id.ToString(),
                "AssociacaoAtleta",
                null,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("torneio", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("{id:guid}/confrontos/gerar")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorConfrontos)}")]
    [ProducesResponseType(typeof(IEnumerable<ConfrontoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ConfrontoDto>>> GenerateMatches(
        Guid id,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            var confrontos = await mediator.Send(new GerarConfrontosCommand(new GerarConfrontosDto(id)));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Confronto",
                id.ToString(),
                "Geracao",
                null,
                confrontos,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(confrontos);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            ModelState.AddModelError("confrontos", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("{id:guid}/confrontos")]
    [ProducesResponseType(typeof(IEnumerable<ConfrontoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConfrontoDto>>> ListMatches(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var confrontos = await mediator.Send(new ObterConfrontosTorneioQuery(id));
        return Ok(confrontos);
    }

    public record CreateTorneioRequest(
        string Nome,
        string? Descricao,
        TipoTorneio Tipo,
        Categoria? Categoria,
        DateTime? DataInicio,
        DateTime? DataFim,
        int? PontuacaoVitoria,
        int? PontuacaoDerrota,
        int? PontuacaoEmpate,
        int? PontuacaoWO,
        bool PermiteEmpate = false);

    public record AddAtletaTorneioRequest(Guid AtletaId);
}
