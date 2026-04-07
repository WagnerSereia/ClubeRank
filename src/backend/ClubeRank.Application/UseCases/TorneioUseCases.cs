using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.Queries;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using MediatR;

namespace ClubeRank.Application.UseCases;

public class TorneioUseCases :
    IRequestHandler<CriarTorneioCommand, Guid>,
    IRequestHandler<AdicionarAtletaTorneioCommand>,
    IRequestHandler<GerarConfrontosCommand, IEnumerable<ConfrontoDto>>,
    IRequestHandler<RegistrarResultadoCommand>,
    IRequestHandler<ObterTorneioPorIdQuery, TorneioDto?>,
    IRequestHandler<ListarTorneiosOrganizacaoQuery, IEnumerable<TorneioDto>>,
    IRequestHandler<ObterConfrontosTorneioQuery, IEnumerable<ConfrontoDto>>
{
    private readonly ITorneioRepository _torneioRepository;
    private readonly IConfrontoRepository _confrontoRepository;
    private readonly IAtletaRepository _atletaRepository;
    private readonly IConfrontoService _confrontoService;
    private readonly IRankingService _rankingService;

    public TorneioUseCases(
        ITorneioRepository torneioRepository,
        IConfrontoRepository confrontoRepository,
        IAtletaRepository atletaRepository,
        IConfrontoService confrontoService,
        IRankingService rankingService)
    {
        _torneioRepository = torneioRepository;
        _confrontoRepository = confrontoRepository;
        _atletaRepository = atletaRepository;
        _confrontoService = confrontoService;
        _rankingService = rankingService;
    }

    public async Task<Guid> Handle(CriarTorneioCommand request, CancellationToken cancellationToken)
    {
        if (request.Torneio.DataInicio.HasValue &&
            request.Torneio.DataFim.HasValue &&
            request.Torneio.DataInicio.Value > request.Torneio.DataFim.Value)
        {
            throw new ArgumentException("A data de início do torneio não pode ser maior que a data de fim.");
        }

        var torneio = new Torneio(
            request.Torneio.Nome,
            request.Torneio.Tipo,
            request.Torneio.OrganizacaoId,
            request.Torneio.Categoria);

        torneio.AtualizarDados(
            request.Torneio.Nome,
            request.Torneio.Descricao,
            request.Torneio.DataInicio,
            request.Torneio.DataFim);
        torneio.AtualizarConfiguracaoPontuacao(
            request.Torneio.PontuacaoVitoria,
            request.Torneio.PontuacaoDerrota,
            request.Torneio.PontuacaoEmpate,
            request.Torneio.PontuacaoWO,
            request.Torneio.PermiteEmpate);

        await _torneioRepository.Adicionar(torneio);
        return torneio.Id;
    }

    public async Task Handle(AdicionarAtletaTorneioCommand request, CancellationToken cancellationToken)
    {
        var torneio = await _torneioRepository.ObterPorId(request.Dados.TorneioId);
        if (torneio is null)
        {
            throw new KeyNotFoundException("Torneio não encontrado");
        }

        var atleta = await _atletaRepository.ObterPorId(request.Dados.AtletaId);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta não encontrado");
        }

        if (atleta.OrganizacaoId != torneio.OrganizacaoId)
        {
            throw new InvalidOperationException("O atleta não pertence à organização do torneio.");
        }

        if (atleta.Status != StatusAtleta.Ativo)
        {
            throw new InvalidOperationException("Somente atletas ativos podem ser associados ao torneio.");
        }

        torneio.AdicionarAtleta(atleta);
        await _torneioRepository.Atualizar(torneio);
    }

    public async Task<IEnumerable<ConfrontoDto>> Handle(GerarConfrontosCommand request, CancellationToken cancellationToken)
    {
        var torneio = await _torneioRepository.ObterPorIdComAtletas(request.Dados.TorneioId);
        if (torneio is null)
        {
            throw new KeyNotFoundException("Torneio não encontrado");
        }

        IEnumerable<Confronto> confrontos;

        if (torneio.Tipo == TipoTorneio.Ladder)
        {
            confrontos = await _confrontoService.GerarConfrontosLadder(torneio);
        }
        else if (torneio.Tipo == TipoTorneio.RoundRobin)
        {
            confrontos = await _confrontoService.GerarConfrontosRoundRobin(torneio);
        }
        else
        {
            throw new NotSupportedException($"Tipo de torneio {torneio.Tipo} não suportado nesta fase");
        }

        foreach (var confronto in confrontos)
        {
            await _confrontoRepository.Adicionar(confronto);
        }

        return confrontos.Select(MapearConfrontoParaDto);
    }

    public async Task Handle(RegistrarResultadoCommand request, CancellationToken cancellationToken)
    {
        var confronto = await _confrontoRepository.ObterPorId(request.Dados.ConfrontoId);
        if (confronto is null)
        {
            throw new KeyNotFoundException("Confronto não encontrado");
        }

        if (request.Dados.TipoResultado == TipoResultado.Empate && confronto.Torneio.Configuracao.PermiteEmpate is false)
        {
            throw new InvalidOperationException("Empate não é permitido para este torneio.");
        }

        var resultado = new ResultadoConfronto(request.Dados.TipoResultado, request.Dados.JustificativaWO);
        confronto.RegistrarResultado(resultado);

        await _confrontoRepository.Atualizar(confronto);

        await _rankingService.AtualizarRankingAposConfronto(confronto, request.Dados.UsuarioId);
    }

    public async Task<TorneioDto?> Handle(ObterTorneioPorIdQuery request, CancellationToken cancellationToken)
    {
        var torneio = await _torneioRepository.ObterPorIdComAtletas(request.TorneioId);
        return torneio is null ? null : MapearTorneioParaDto(torneio);
    }

    public async Task<IEnumerable<TorneioDto>> Handle(ListarTorneiosOrganizacaoQuery request, CancellationToken cancellationToken)
    {
        var torneios = await _torneioRepository.ListarPorOrganizacao(request.OrganizacaoId);
        return torneios.Select(MapearTorneioParaDto);
    }

    public async Task<IEnumerable<ConfrontoDto>> Handle(ObterConfrontosTorneioQuery request, CancellationToken cancellationToken)
    {
        var confrontos = await _confrontoRepository.ListarPorTorneio(request.TorneioId);
        return confrontos.Select(MapearConfrontoParaDto);
    }

    private static TorneioDto MapearTorneioParaDto(Torneio torneio)
    {
        return new TorneioDto(
            torneio.Id,
            torneio.Nome,
            torneio.Descricao,
            torneio.Tipo,
            torneio.Status,
            torneio.OrganizacaoId,
            torneio.Categoria,
            torneio.DataInicio,
            torneio.DataFim,
            torneio.Configuracao.PontuacaoVitoria,
            torneio.Configuracao.PontuacaoDerrota,
            torneio.Configuracao.PontuacaoEmpate,
            torneio.Configuracao.PontuacaoWO,
            torneio.Configuracao.PermiteEmpate,
            torneio.Atletas.Count,
            torneio.DataCriacao);
    }

    private static ConfrontoDto MapearConfrontoParaDto(Confronto confronto)
    {
        return new ConfrontoDto(
            confronto.Id,
            confronto.AtletaAId,
            confronto.AtletaA?.Nome.NomeFormatado ?? "N/A",
            confronto.AtletaBId,
            confronto.AtletaB?.Nome.NomeFormatado ?? "N/A",
            confronto.TorneioId,
            confronto.Status,
            confronto.Resultado?.Tipo,
            confronto.DataAgendamento,
            confronto.Notas);
    }
}
