using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Services;

public record ListarAtletasDto(
    int Pagina,
    int TamanhoPagina,
    string? FiltroNome,
    StatusAtleta? FiltroStatus,
    Categoria? FiltroCategoria,
    Genero? FiltroGenero,
    string? Ordenacao
);