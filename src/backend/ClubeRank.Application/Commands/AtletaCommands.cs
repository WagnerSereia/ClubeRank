using ClubeRank.Application.DTOs;
using ClubeRank.Domain.ValueObjects;
using MediatR;

namespace ClubeRank.Application.Commands;

public record CriarAtletaCommand(CriarAtletaDto Atleta) : IRequest<Guid>;

public record AtualizarAtletaCommand(AtualizarAtletaDto Atleta) : IRequest;

public record InativarAtletaCommand(Guid AtletaId) : IRequest;

public record ReativarAtletaCommand(Guid AtletaId) : IRequest;

public record AlterarCategoriaAtletaCommand(Guid AtletaId, Categoria Categoria) : IRequest;
