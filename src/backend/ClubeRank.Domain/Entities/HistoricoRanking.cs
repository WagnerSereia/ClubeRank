using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Entities;

public class HistoricoRanking : Entity
{
    public Guid AtletaId { get; private set; }
    public Pontuacao PontuacaoAntes { get; private set; } = null!;
    public Pontuacao PontuacaoDepois { get; private set; } = null!;
    public Guid? ConfrontoId { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }

    // Navegação
    public Atleta Atleta { get; private set; } = null!;
    public Confronto? Confronto { get; private set; }
    private HistoricoRanking() { } // Para EF Core

    public HistoricoRanking(Guid atletaId, Guid tenantId, Pontuacao pontuacaoAntes, Pontuacao pontuacaoDepois,
                           Guid? confrontoId, string motivo, Guid usuarioId)
    {
        AtletaId = atletaId;
        PontuacaoAntes = pontuacaoAntes ?? throw new ArgumentNullException(nameof(pontuacaoAntes));
        PontuacaoDepois = pontuacaoDepois ?? throw new ArgumentNullException(nameof(pontuacaoDepois));
        ConfrontoId = confrontoId;
        Motivo = motivo ?? throw new ArgumentNullException(nameof(motivo));
        UsuarioId = usuarioId;
        TenantId = tenantId; // Tenant sempre representa a organização
    }
}
