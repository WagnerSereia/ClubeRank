using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.Queries;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.ValueObjects;
using MediatR;

namespace ClubeRank.Application.UseCases;

public class OrganizacaoUseCases :
    IRequestHandler<CriarOrganizacaoCommand, Guid>,
    IRequestHandler<ObterOrganizacaoPorIdQuery, OrganizacaoDto?>,
    IRequestHandler<ListarOrganizacoesQuery, IEnumerable<OrganizacaoDto>>
{
    private readonly IOrganizacaoRepository _organizacaoRepository;
    public OrganizacaoUseCases(
        IOrganizacaoRepository organizacaoRepository)
    {
        _organizacaoRepository = organizacaoRepository;
    }

    public async Task<Guid> Handle(CriarOrganizacaoCommand request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Organizacao.Email);

        var organizacao = new Organizacao(
            request.Organizacao.Nome,
            email,
            request.Organizacao.Telefone,
            request.Organizacao.Modalidade,
            request.Organizacao.Plano);

        await _organizacaoRepository.Adicionar(organizacao);

        // Criar usuário administrador padrão
        return organizacao.Id;
    }

    public async Task<OrganizacaoDto?> Handle(ObterOrganizacaoPorIdQuery request, CancellationToken cancellationToken)
    {
        var organizacao = await _organizacaoRepository.ObterPorId(request.OrganizacaoId);
        return organizacao is null ? null : MapearParaDto(organizacao);
    }

    public async Task<IEnumerable<OrganizacaoDto>> Handle(ListarOrganizacoesQuery request, CancellationToken cancellationToken)
    {
        var organizacoes = await _organizacaoRepository.Listar();
        return organizacoes.Select(MapearParaDto);
    }

    private static OrganizacaoDto MapearParaDto(Organizacao organizacao)
    {
        return new OrganizacaoDto(
            organizacao.Id,
            organizacao.Nome,
            organizacao.Email.Valor,
            organizacao.Telefone,
            organizacao.Modalidade,
            organizacao.Status,
            organizacao.Plano,
            organizacao.LogoUrl,
            organizacao.DataCriacao
        );
    }
}

// Interface movida para ClubeRank.Domain.Interfaces.IRepositories
