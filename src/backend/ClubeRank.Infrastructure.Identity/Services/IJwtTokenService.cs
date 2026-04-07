using ClubeRank.Infrastructure.Identity.Entities;

namespace ClubeRank.Infrastructure.Identity.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(Usuario usuario);
}
