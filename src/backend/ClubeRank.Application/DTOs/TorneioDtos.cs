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
    int? PontuacaoSetVencido = null,
    int? MelhorDeSets = null,
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
    int PontuacaoSetVencido,
    int MelhorDeSets,
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
    int TotalSetsAtletaA,
    int TotalSetsAtletaB,
    int TotalGamesAtletaA,
    int TotalGamesAtletaB,
    IReadOnlyCollection<SetResultadoDto> Sets,
    DateTime DataAgendamento,
    string? Notas
);

public record RegistrarResultadoDto(
    Guid ConfrontoId,
    TipoResultado? TipoResultado,
    IReadOnlyCollection<SetResultadoDto>? Sets,
    string? JustificativaWO,
    Guid UsuarioId
);

public record SetResultadoDto(
    int Numero,
    int GamesAtletaA,
    int GamesAtletaB,
    bool TieBreak = false
);
