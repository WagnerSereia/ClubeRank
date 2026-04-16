using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Application.DTOs;

public record CriarAtletaDto(
    string PrimeiroNome,
    string Sobrenome,
    Genero Genero,
    string Email,
    string? Telefone,
    Categoria Categoria,
    Guid OrganizacaoId
);

public record AtualizarAtletaDto(
    Guid Id,
    string PrimeiroNome,
    string Sobrenome,
    string? Telefone
);

public record AtletaDto(
    Guid Id,
    string NomeCompleto,
    Genero Genero,
    string Email,
    string? Telefone,
    StatusAtleta Status,
    Categoria Categoria,
    int PontuacaoAtual,
    int TotalSetsVencidos,
    int TotalGamesVencidos,
    int SaldoGames,
    DateTime DataAtualizacaoPontuacao,
    Guid OrganizacaoId,
    DateTime DataCriacao
);

public record ListarAtletasDto(
    int Pagina,
    int TamanhoPagina,
    string? FiltroNome,
    StatusAtleta? FiltroStatus,
    Categoria? FiltroCategoria,
    Genero? FiltroGenero,
    string? Ordenacao
);
