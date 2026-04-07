namespace ClubeRank.Application.DTOs.Auth;

public record TokenResponseDto(
    string AccessToken,
    Guid TenantId,
    IReadOnlyCollection<string> Roles);
