using ClubeRank.Application.DTOs;
using MediatR;

namespace ClubeRank.Application.Commands;

public record CriarOrganizacaoCommand(CriarOrganizacaoDto Organizacao) : IRequest<Guid>;

public record CriarTorneioCommand(CriarTorneioDto Torneio) : IRequest<Guid>;

public record AdicionarAtletaTorneioCommand(AdicionarAtletaTorneioDto Dados) : IRequest;

public record GerarConfrontosCommand(GerarConfrontosDto Dados) : IRequest<IEnumerable<ConfrontoDto>>;

public record RegistrarResultadoCommand(RegistrarResultadoDto Dados) : IRequest;