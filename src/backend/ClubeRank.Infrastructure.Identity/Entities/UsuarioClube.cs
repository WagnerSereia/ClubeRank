using ClubeRank.Domain.Interfaces;

namespace ClubeRank.Infrastructure.Identity.Entities;

public class UsuarioClube : ITenantEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public Guid ClubeId { get; set; }
    public Guid TenantId { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Clube Clube { get; set; } = null!;
}
