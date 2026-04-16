using ClubeRank.Application.DTOs.Auth;
using ClubeRank.Infrastructure.Identity.Data;
using ClubeRank.Infrastructure.Identity.Entities;
using ClubeRank.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubeRank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "acesso")]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponseDto>> GenerateToken(
        [FromServices] IdentityDbContext identityDbContext,
        [FromServices] UserManager<Usuario> userManager,
        [FromServices] IJwtTokenService jwtTokenService,
        [FromBody] LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await identityDbContext.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

        if (user is null)
        {
            return Unauthorized();
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Unauthorized();
        }

        var token = await jwtTokenService.GenerateTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        return Ok(new TokenResponseDto(token, user.TenantId, roles.ToArray()));
    }
}
