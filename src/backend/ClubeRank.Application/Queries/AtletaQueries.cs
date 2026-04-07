using ClubeRank.Application.DTOs;
using ClubeRank.Domain.ValueObjects;
using MediatR;

namespace ClubeRank.Application.Queries;

public record ObterAtletaPorIdQuery(Guid AtletaId) : IRequest<AtletaDto?>;

public record ListarAtletasQuery(ListarAtletasDto Filtros) : IRequest<IEnumerable<AtletaDto>>;

public record ObterRankingQuery(Guid OrganizacaoId, Categoria? Categoria, Genero? Genero, int? PeriodoDias = null) : IRequest<IEnumerable<AtletaDto>>;

public record ObterHistoricoAtletaQuery(Guid AtletaId) : IRequest<IEnumerable<HistoricoRankingDto>>;

public record HistoricoRankingDto(
    Guid Id,
    int PontuacaoAntes,
    int PontuacaoDepois,
    DateTime DataAtualizacao,
    string Motivo,
    Guid? ConfrontoId,
    string? NomeAdversario,
    TipoResultado? Resultado);
