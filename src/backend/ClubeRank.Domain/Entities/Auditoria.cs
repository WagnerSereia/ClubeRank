using ClubeRank.Domain.Constants;

namespace ClubeRank.Domain.Entities;

public class Auditoria : Entity
{
    public Guid? OrganizacaoId { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public string Entidade { get; private set; }
    public string EntidadeId { get; private set; }
    public string Acao { get; private set; }
    public string? DadosAntes { get; private set; }
    public string? DadosDepois { get; private set; }
    public string? Ip { get; private set; }

    private Auditoria()
    {
        Entidade = string.Empty;
        EntidadeId = string.Empty;
        Acao = string.Empty;
    }

    public Auditoria(
        Guid? organizacaoId,
        Guid? usuarioId,
        string entidade,
        string entidadeId,
        string acao,
        string? dadosAntes,
        string? dadosDepois,
        string? ip)
    {
        OrganizacaoId = organizacaoId;
        UsuarioId = usuarioId;
        Entidade = entidade ?? throw new ArgumentNullException(nameof(entidade));
        EntidadeId = entidadeId ?? throw new ArgumentNullException(nameof(entidadeId));
        Acao = acao ?? throw new ArgumentNullException(nameof(acao));
        DadosAntes = dadosAntes;
        DadosDepois = dadosDepois;
        Ip = ip;
        TenantId = organizacaoId ?? SystemTenants.GlobalTenantId;
    }
}
