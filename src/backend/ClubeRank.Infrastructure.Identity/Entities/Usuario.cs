using ClubeRank.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ClubeRank.Infrastructure.Identity.Entities;

public class Usuario : IdentityUser, ITenantEntity
{
    public string NomeCompleto { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    public ICollection<UsuarioClube> UsuarioClubes { get; set; } = new List<UsuarioClube>();
}
