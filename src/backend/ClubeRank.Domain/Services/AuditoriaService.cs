using System.Text.Json;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;

namespace ClubeRank.Domain.Services;

public interface IAuditoriaService
{
    Task RegistrarAsync(
        Guid? organizacaoId,
        Guid? usuarioId,
        string entidade,
        string entidadeId,
        string acao,
        object? dadosAntes,
        object? dadosDepois,
        string? ip);
}

public class AuditoriaService : IAuditoriaService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuditoriaService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task RegistrarAsync(
        Guid? organizacaoId,
        Guid? usuarioId,
        string entidade,
        string entidadeId,
        string acao,
        object? dadosAntes,
        object? dadosDepois,
        string? ip)
    {
        var auditoria = new Auditoria(
            organizacaoId,
            usuarioId,
            entidade,
            entidadeId,
            acao,
            dadosAntes is null ? null : JsonSerializer.Serialize(dadosAntes, JsonOptions),
            dadosDepois is null ? null : JsonSerializer.Serialize(dadosDepois, JsonOptions),
            ip);

        await _auditoriaRepository.Adicionar(auditoria);
    }
}
