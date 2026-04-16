using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.Queries;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;
using MediatR;

namespace ClubeRank.Application.UseCases;

public class AtletaUseCases :
    IRequestHandler<CriarAtletaCommand, Guid>,
    IRequestHandler<AtualizarAtletaCommand>,
    IRequestHandler<InativarAtletaCommand>,
    IRequestHandler<ReativarAtletaCommand>,
    IRequestHandler<AlterarCategoriaAtletaCommand>,
    IRequestHandler<ObterAtletaPorIdQuery, AtletaDto?>,
    IRequestHandler<ListarAtletasQuery, IEnumerable<AtletaDto>>,
    IRequestHandler<ObterRankingQuery, IEnumerable<AtletaDto>>,
    IRequestHandler<ObterHistoricoAtletaQuery, IEnumerable<HistoricoRankingDto>>
{
    private readonly IAtletaRepository _atletaRepository;
    private readonly IHistoricoRankingRepository _historicoRepository;
    private readonly IConfrontoRepository _confrontoRepository;
    private readonly IRankingService _rankingService;

    public AtletaUseCases(
        IAtletaRepository atletaRepository,
        IHistoricoRankingRepository historicoRepository,
        IConfrontoRepository confrontoRepository,
        IRankingService rankingService)
    {
        _atletaRepository = atletaRepository;
        _historicoRepository = historicoRepository;
        _confrontoRepository = confrontoRepository;
        _rankingService = rankingService;
    }

    public async Task<Guid> Handle(CriarAtletaCommand request, CancellationToken cancellationToken)
    {
        var nome = new NomeCompleto(request.Atleta.PrimeiroNome, request.Atleta.Sobrenome);
        var email = new Email(request.Atleta.Email);

        var emailJaExiste = await _atletaRepository.ExisteEmailNaOrganizacao(request.Atleta.OrganizacaoId, email.Valor);
        if (emailJaExiste)
        {
            throw new InvalidOperationException("Ja existe atleta com este email na organizacao.");
        }

        var atleta = new Atleta(
            nome,
            request.Atleta.Genero,
            email,
            request.Atleta.Telefone,
            request.Atleta.Categoria,
            request.Atleta.OrganizacaoId);

        await _atletaRepository.Adicionar(atleta);
        return atleta.Id;
    }

    public async Task Handle(AtualizarAtletaCommand request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepository.ObterPorId(request.Atleta.Id);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta nao encontrado");
        }

        var nome = new NomeCompleto(request.Atleta.PrimeiroNome, request.Atleta.Sobrenome);
        atleta.AlterarDadosPessoais(nome, request.Atleta.Telefone);

        await _atletaRepository.Atualizar(atleta);
    }

    public async Task Handle(InativarAtletaCommand request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepository.ObterPorId(request.AtletaId);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta nao encontrado");
        }

        atleta.Inativar();
        await _atletaRepository.Atualizar(atleta);
    }

    public async Task Handle(ReativarAtletaCommand request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepository.ObterPorId(request.AtletaId);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta nao encontrado");
        }

        atleta.Ativar();
        await _atletaRepository.Atualizar(atleta);
    }

    public async Task Handle(AlterarCategoriaAtletaCommand request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepository.ObterPorId(request.AtletaId);
        if (atleta is null)
        {
            throw new KeyNotFoundException("Atleta nao encontrado");
        }

        atleta.AlterarCategoria(request.Categoria);
        await _atletaRepository.Atualizar(atleta);
    }

    public async Task<AtletaDto?> Handle(ObterAtletaPorIdQuery request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepository.ObterPorId(request.AtletaId);
        return atleta is null ? null : MapearParaDto(atleta);
    }

    public async Task<IEnumerable<AtletaDto>> Handle(ListarAtletasQuery request, CancellationToken cancellationToken)
    {
        var filtros = new Domain.Services.ListarAtletasDto(
            request.Filtros.Pagina,
            request.Filtros.TamanhoPagina,
            request.Filtros.FiltroNome,
            request.Filtros.FiltroStatus,
            request.Filtros.FiltroCategoria,
            request.Filtros.FiltroGenero,
            request.Filtros.Ordenacao);

        var atletas = await _atletaRepository.Listar(filtros);
        return atletas.Select(x => MapearParaDto(x));
    }

    public async Task<IEnumerable<AtletaDto>> Handle(ObterRankingQuery request, CancellationToken cancellationToken)
    {
        var atletas = (await _rankingService.ObterRankingPorCategoria(
            request.OrganizacaoId,
            request.Categoria,
            request.Genero)).ToList();

        DateTime? dataInicial = null;
        if (request.PeriodoDias.HasValue && request.PeriodoDias.Value > 0)
        {
            dataInicial = DateTime.UtcNow.AddDays(-request.PeriodoDias.Value);
            var atletaIdsNoPeriodo = await _historicoRepository.ObterAtletaIdsComHistoricoNoPeriodo(request.OrganizacaoId, dataInicial.Value);
            atletas = atletas.Where(x => atletaIdsNoPeriodo.Contains(x.Id)).ToList();
        }

        var confrontos = await _confrontoRepository.ListarRealizadosPorOrganizacao(request.OrganizacaoId);
        if (dataInicial.HasValue)
        {
            confrontos = confrontos.Where(x => x.DataAgendamento >= dataInicial.Value).ToArray();
        }

        var estatisticas = CalcularEstatisticasRanking(atletas.Select(x => x.Id), confrontos);

        return atletas
            .OrderByDescending(x => x.PontuacaoAtual.Valor)
            .ThenByDescending(x => estatisticas.GetValueOrDefault(x.Id)?.TotalSetsVencidos ?? 0)
            .ThenByDescending(x => estatisticas.GetValueOrDefault(x.Id)?.SaldoGames ?? 0)
            .ThenByDescending(x => estatisticas.GetValueOrDefault(x.Id)?.TotalGamesVencidos ?? 0)
            .ThenBy(x => x.Nome.PrimeiroNome)
            .ThenBy(x => x.Nome.Sobrenome)
            .Select(x => MapearParaDto(x, estatisticas.GetValueOrDefault(x.Id)))
            .ToArray();
    }

    public async Task<IEnumerable<HistoricoRankingDto>> Handle(ObterHistoricoAtletaQuery request, CancellationToken cancellationToken)
    {
        var historicos = await _historicoRepository.ObterPorAtletaId(request.AtletaId);
        return historicos.Select(h => MapearHistoricoParaDto(h, request.AtletaId));
    }

    private static AtletaDto MapearParaDto(Atleta atleta, RankingEstatisticaDto? estatistica = null)
    {
        return new AtletaDto(
            atleta.Id,
            atleta.Nome.NomeFormatado,
            atleta.Genero,
            atleta.Email.Valor,
            atleta.Telefone,
            atleta.Status,
            atleta.Categoria,
            atleta.PontuacaoAtual.Valor,
            estatistica?.TotalSetsVencidos ?? 0,
            estatistica?.TotalGamesVencidos ?? 0,
            estatistica?.SaldoGames ?? 0,
            atleta.PontuacaoAtual.DataAtualizacao,
            atleta.OrganizacaoId,
            atleta.DataCriacao);
    }

    private static HistoricoRankingDto MapearHistoricoParaDto(HistoricoRanking historico, Guid atletaId)
    {
        return new HistoricoRankingDto(
            historico.Id,
            historico.PontuacaoAntes.Valor,
            historico.PontuacaoDepois.Valor,
            historico.PontuacaoDepois.DataAtualizacao,
            historico.Motivo,
            historico.ConfrontoId,
            ObterNomeAdversario(historico, atletaId),
            historico.Confronto?.Resultado?.Tipo);
    }

    private static string? ObterNomeAdversario(HistoricoRanking historico, Guid atletaId)
    {
        if (historico.Confronto is null)
        {
            return null;
        }

        if (historico.Confronto.AtletaAId == atletaId)
        {
            return historico.Confronto.AtletaB?.Nome.NomeFormatado;
        }

        if (historico.Confronto.AtletaBId == atletaId)
        {
            return historico.Confronto.AtletaA?.Nome.NomeFormatado;
        }

        return null;
    }

    private static Dictionary<Guid, RankingEstatisticaDto> CalcularEstatisticasRanking(
        IEnumerable<Guid> atletaIds,
        IEnumerable<Confronto> confrontos)
    {
        var filtroAtletas = atletaIds.ToHashSet();
        var estatisticas = filtroAtletas.ToDictionary(
            atletaId => atletaId,
            _ => new RankingEstatisticaDto(0, 0, 0));

        foreach (var confronto in confrontos.Where(x => x.Resultado is not null))
        {
            AcumularEstatisticas(estatisticas, filtroAtletas, confronto.AtletaAId, confronto.Resultado!.TotalSetsVencidosAtletaA, confronto.Resultado.TotalGamesAtletaA, confronto.Resultado.TotalGamesAtletaA - confronto.Resultado.TotalGamesAtletaB);
            AcumularEstatisticas(estatisticas, filtroAtletas, confronto.AtletaBId, confronto.Resultado!.TotalSetsVencidosAtletaB, confronto.Resultado.TotalGamesAtletaB, confronto.Resultado.TotalGamesAtletaB - confronto.Resultado.TotalGamesAtletaA);
        }

        return estatisticas;
    }

    private static void AcumularEstatisticas(
        Dictionary<Guid, RankingEstatisticaDto> estatisticas,
        HashSet<Guid> filtroAtletas,
        Guid atletaId,
        int setsVencidos,
        int gamesVencidos,
        int saldoGames)
    {
        if (!filtroAtletas.Contains(atletaId))
        {
            return;
        }

        var atual = estatisticas[atletaId];
        estatisticas[atletaId] = atual with
        {
            TotalSetsVencidos = atual.TotalSetsVencidos + setsVencidos,
            TotalGamesVencidos = atual.TotalGamesVencidos + gamesVencidos,
            SaldoGames = atual.SaldoGames + saldoGames
        };
    }

    private record RankingEstatisticaDto(
        int TotalSetsVencidos,
        int TotalGamesVencidos,
        int SaldoGames);
}
