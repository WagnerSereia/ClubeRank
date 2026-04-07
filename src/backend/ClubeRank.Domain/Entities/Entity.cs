using ClubeRank.Domain.Interfaces;

namespace ClubeRank.Domain.Entities;

public abstract class Entity : ITenantEntity
{
    public Guid Id { get; protected set; }
    public Guid TenantId { get; set; }
    public DateTime DataCriacao { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        Id = id;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity a, Entity b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b) => !(a == b);
}