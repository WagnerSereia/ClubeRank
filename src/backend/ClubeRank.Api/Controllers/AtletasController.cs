using ClubeRank.Api.Extensions;
using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.Queries;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ListarAtletasRequestDto = ClubeRank.Application.DTOs.ListarAtletasDto;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "atletas")]
[Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)},{nameof(PerfilUsuario.Auditor)}")]
public class AtletasController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AtletaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AtletaDto>>> List(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? nome = null,
        [FromQuery] StatusAtleta? status = null,
        [FromQuery] Categoria? categoria = null,
        [FromQuery] Genero? genero = null,
        [FromQuery] string? ordenacao = null)
    {
        var query = new ListarAtletasQuery(new ListarAtletasRequestDto(
            pageNumber,
            pageSize,
            nome,
            status,
            categoria,
            genero,
            ordenacao));

        var atletas = await mediator.Send(query);
        return Ok(atletas);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AtletaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtletaDto>> GetById(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var atleta = await mediator.Send(new ObterAtletaPorIdQuery(id));
        if (atleta is null)
        {
            return NotFound();
        }

        return Ok(atleta);
    }

    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoRankingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HistoricoRankingDto>>> GetHistory(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var historico = await mediator.Send(new ObterHistoricoAtletaQuery(id));
        return Ok(historico);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(
        [FromBody] CreateAtletaRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return BadRequest("Tenant do usuário autenticado não encontrado.");
        }

        try
        {
            var dto = new CriarAtletaDto(
                request.PrimeiroNome.Trim(),
                request.Sobrenome.Trim(),
                request.Genero,
                request.Email.Trim(),
                request.Telefone?.Trim(),
                request.Categoria,
                tenantId);

            var atletaId = await mediator.Send(new CriarAtletaCommand(dto));
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Atleta",
                atletaId.ToString(),
                "Criacao",
                null,
                dto,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return CreatedAtAction(nameof(GetById), new { id = atletaId }, new { id = atletaId });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError("atleta", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAtletaRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            var atual = await mediator.Send(new ObterAtletaPorIdQuery(id));
            await mediator.Send(new AtualizarAtletaCommand(new AtualizarAtletaDto(
                id,
                request.PrimeiroNome.Trim(),
                request.Sobrenome.Trim(),
                request.Telefone?.Trim())));
            var depois = await mediator.Send(new ObterAtletaPorIdQuery(id));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Atleta",
                id.ToString(),
                "Edicao",
                atual,
                depois,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("atleta", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPatch("{id:guid}/inativar")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Inactivate(
        Guid id,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            var atual = await mediator.Send(new ObterAtletaPorIdQuery(id));
            await mediator.Send(new InativarAtletaCommand(id));
            var depois = await mediator.Send(new ObterAtletaPorIdQuery(id));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Atleta",
                id.ToString(),
                "Inativacao",
                atual,
                depois,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/reativar")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reactivate(
        Guid id,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            var atual = await mediator.Send(new ObterAtletaPorIdQuery(id));
            await mediator.Send(new ReativarAtletaCommand(id));
            var depois = await mediator.Send(new ObterAtletaPorIdQuery(id));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Atleta",
                id.ToString(),
                "Reativacao",
                atual,
                depois,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/categoria")]
    [Authorize(Roles = $"{nameof(PerfilUsuario.SuperAdmin)},{nameof(PerfilUsuario.AdminOrganizacao)},{nameof(PerfilUsuario.GestorAtletas)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeCategory(
        Guid id,
        [FromBody] ChangeCategoriaAtletaRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IAuditoriaService auditoriaService)
    {
        try
        {
            var atual = await mediator.Send(new ObterAtletaPorIdQuery(id));
            await mediator.Send(new AlterarCategoriaAtletaCommand(id, request.Categoria));
            var depois = await mediator.Send(new ObterAtletaPorIdQuery(id));
            var tenantId = User.GetTenantIdOrNull();
            var usuarioId = User.GetUserIdOrSystem();
            await auditoriaService.RegistrarAsync(
                tenantId,
                usuarioId,
                "Atleta",
                id.ToString(),
                "AlteracaoCategoria",
                atual,
                depois,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("categoria", ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    public record CreateAtletaRequest(
        string PrimeiroNome,
        string Sobrenome,
        Genero Genero,
        string Email,
        string? Telefone,
        Categoria Categoria);

    public record UpdateAtletaRequest(
        string PrimeiroNome,
        string Sobrenome,
        string? Telefone);

    public record ChangeCategoriaAtletaRequest(Categoria Categoria);
}
