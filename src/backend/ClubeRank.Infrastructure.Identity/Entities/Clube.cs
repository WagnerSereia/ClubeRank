using ClubeRank.Domain.Interfaces;

namespace ClubeRank.Infrastructure.Identity.Entities;

public class Clube : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<UsuarioClube> UsuarioClubes { get; set; } = new List<UsuarioClube>();
}
