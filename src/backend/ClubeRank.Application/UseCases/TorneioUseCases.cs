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
            throw new ArgumentException("A data de inicio do torneio nao pode ser maior que a data de fim.");
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
            request.Torneio.PontuacaoSetVencido,
            request.Torneio.MelhorDeSets,
            request.Torneio.PermiteEmpate);

        await _torneioRepository.Adicionar(torneio);
        return torneio.Id;
    }

    public async Task Handle(AdicionarAtletaTorneioCommand request, CancellationToken cancellationToken)
    {
        var torneio = await _torneioRepository.ObterPorId(request.Dados.TorneioId);
        if (torneio is null)
        {
            throw new KeyNotFoundException("Torneio nao encontrado");
        }

        var atleta = await _atletaRepository.ObterPorId(request.Dados.AtletaId);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta nao encontrado");
        }

        if (atleta.OrganizacaoId != torneio.OrganizacaoId)
        {
            throw new InvalidOperationException("O atleta nao pertence a organizacao do torneio.");
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
            throw new KeyNotFoundException("Torneio nao encontrado");
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
            throw new NotSupportedException($"Tipo de torneio {torneio.Tipo} nao suportado nesta fase");
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
            throw new KeyNotFoundException("Confronto nao encontrado");
        }

        var resultado = CriarResultado(request.Dados, confronto.Torneio);
        if (resultado.Tipo == TipoResultado.Empate && confronto.Torneio.Configuracao.PermiteEmpate is false)
        {
            throw new InvalidOperationException("Empate nao e permitido para este torneio.");
        }

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
            torneio.Configuracao.PontuacaoSetVencido,
            torneio.Configuracao.MelhorDeSets,
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
            confronto.Resultado?.TotalSetsVencidosAtletaA ?? 0,
            confronto.Resultado?.TotalSetsVencidosAtletaB ?? 0,
            confronto.Resultado?.TotalGamesAtletaA ?? 0,
            confronto.Resultado?.TotalGamesAtletaB ?? 0,
            confronto.Resultado?.Sets.Select(MapearSetParaDto).ToArray() ?? [],
            confronto.DataAgendamento,
            confronto.Notas);
    }

    private static ResultadoConfronto CriarResultado(RegistrarResultadoDto dados, Torneio torneio)
    {
        var sets = dados.Sets?
            .Select(x => new SetConfronto(x.Numero, x.GamesAtletaA, x.GamesAtletaB, x.TieBreak))
            .ToArray() ?? [];

        ResultadoConfronto resultado;

        if (sets.Length > 0)
        {
            resultado = ResultadoConfronto.CriarComSets(sets, dados.JustificativaWO);
            ValidarFormatoSets(torneio, resultado);

            if (dados.TipoResultado.HasValue && dados.TipoResultado.Value != resultado.Tipo)
            {
                throw new InvalidOperationException("O tipo de resultado informado nao corresponde ao placar dos sets.");
            }
        }
        else
        {
            if (!dados.TipoResultado.HasValue)
            {
                throw new InvalidOperationException("Informe o tipo de resultado ou o placar detalhado dos sets.");
            }

            resultado = new ResultadoConfronto(dados.TipoResultado.Value, null, dados.JustificativaWO);
        }

        return resultado;
    }

    private static void ValidarFormatoSets(Torneio torneio, ResultadoConfronto resultado)
    {
        if (resultado.Sets.Count > torneio.Configuracao.MelhorDeSets)
        {
            throw new InvalidOperationException($"O confronto aceita no maximo {torneio.Configuracao.MelhorDeSets} sets.");
        }

        if (resultado.Tipo == TipoResultado.WO)
        {
            return;
        }

        var setsNecessarios = torneio.Configuracao.ObterSetsNecessariosParaVitoria();
        var setsAtletaA = resultado.TotalSetsVencidosAtletaA;
        var setsAtletaB = resultado.TotalSetsVencidosAtletaB;

        if (resultado.Tipo == TipoResultado.Empate)
        {
            if (!torneio.Configuracao.PermiteEmpate)
            {
                throw new InvalidOperationException("Empate nao e permitido para este torneio.");
            }

            if (setsAtletaA != setsAtletaB)
            {
                throw new InvalidOperationException("Empate exige a mesma quantidade de sets vencidos para ambos os atletas.");
            }

            return;
        }

        var totalSetsVencedor = Math.Max(setsAtletaA, setsAtletaB);
        if (totalSetsVencedor != setsNecessarios)
        {
            throw new InvalidOperationException(
                $"Para melhor de {torneio.Configuracao.MelhorDeSets}, o vencedor precisa vencer {setsNecessarios} sets.");
        }
    }

    private static SetResultadoDto MapearSetParaDto(SetConfronto set)
    {
        return new SetResultadoDto(set.Numero, set.GamesAtletaA, set.GamesAtletaB, set.TieBreak);
    }
}
