namespace ClubeRank.Application.DTOs.Common;

public record PaginationRequestDto(int PageNumber = 1, int PageSize = 20);
