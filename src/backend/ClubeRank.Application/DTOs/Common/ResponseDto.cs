namespace ClubeRank.Application.DTOs.Common;

public record ResponseDto<T>(T? Data, IReadOnlyCollection<ErrorDetailDto>? Errors = null)
{
    public bool Success => Errors is null || Errors.Count == 0;
}
