namespace ClubeRank.Application.DTOs.Common;

public record PagedResponseDto<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);
