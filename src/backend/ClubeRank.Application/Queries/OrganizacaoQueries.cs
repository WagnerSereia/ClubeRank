using ClubeRank.Application.DTOs;
using MediatR;

namespace ClubeRank.Application.Queries;

public record ObterOrganizacaoPorIdQuery(Guid OrganizacaoId) : IRequest<OrganizacaoDto?>;

public record ListarOrganizacoesQuery : IRequest<IEnumerable<OrganizacaoDto>>;

public record ObterTorneioPorIdQuery(Guid TorneioId) : IRequest<TorneioDto?>;

public record ListarTorneiosOrganizacaoQuery(Guid OrganizacaoId) : IRequest<IEnumerable<TorneioDto>>;

public record ObterConfrontosTorneioQuery(Guid TorneioId) : IRequest<IEnumerable<ConfrontoDto>>;