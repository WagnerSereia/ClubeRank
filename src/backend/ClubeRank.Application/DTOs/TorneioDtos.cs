using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Application.DTOs;

public record CriarTorneioDto(
    string Nome,
    string? Descricao,
    TipoTorneio Tipo,
    Guid OrganizacaoId,
    Categoria? Categoria,
    DateTime? DataInicio,
    DateTime? DataFim,
    int? PontuacaoVitoria = null,
    int? PontuacaoDerrota = null,
    int? PontuacaoEmpate = null,
    int? PontuacaoWO = null,
    bool PermiteEmpate = false
);

public record TorneioDto(
    Guid Id,
    string Nome,
    string? Descricao,
    TipoTorneio Tipo,
    StatusTorneio Status,
    Guid OrganizacaoId,
    Categoria? Categoria,
    DateTime? DataInicio,
    DateTime? DataFim,
    int PontuacaoVitoria,
    int PontuacaoDerrota,
    int PontuacaoEmpate,
    int PontuacaoWO,
    bool PermiteEmpate,
    int QuantidadeAtletas,
    DateTime DataCriacao
);

public record AdicionarAtletaTorneioDto(
    Guid TorneioId,
    Guid AtletaId
);

public record GerarConfrontosDto(
    Guid TorneioId
);

public record ConfrontoDto(
    Guid Id,
    Guid AtletaAId,
    string NomeAtletaA,
    Guid AtletaBId,
    string NomeAtletaB,
    Guid TorneioId,
    StatusConfronto Status,
    TipoResultado? Resultado,
    DateTime DataAgendamento,
    string? Notas
);

public record RegistrarResultadoDto(
    Guid ConfrontoId,
    TipoResultado TipoResultado,
    string? JustificativaWO,
    Guid UsuarioId
);
