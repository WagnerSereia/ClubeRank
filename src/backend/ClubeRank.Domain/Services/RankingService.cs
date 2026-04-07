using ClubeRank.Domain.Constants;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Interfaces;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Services;

public interface IRankingService
{
    Task AtualizarRankingAposConfronto(Confronto confronto, Guid usuarioId);
    Task AplicarDecaimentoAtletasInativos();
    Task<IEnumerable<Atleta>> ObterRankingPorCategoria(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null);
}

public class RankingService : IRankingService
{
    private readonly IAtletaRepository _atletaRepository;
    private readonly IHistoricoRankingRepository _historicoRepository;
    private readonly IOrganizacaoRepository _organizacaoRepository;

    public RankingService(
        IAtletaRepository atletaRepository,
        IHistoricoRankingRepository historicoRepository,
        IOrganizacaoRepository organizacaoRepository)
    {
        _atletaRepository = atletaRepository;
        _historicoRepository = historicoRepository;
        _organizacaoRepository = organizacaoRepository;
    }

    public async Task AtualizarRankingAposConfronto(Confronto confronto, Guid usuarioId)
    {
        if (confronto.Resultado is null)
        {
            throw new InvalidOperationException("Confronto deve ter resultado registrado");
        }

        var atletaA = await _atletaRepository.ObterPorId(confronto.AtletaAId);
        var atletaB = await _atletaRepository.ObterPorId(confronto.AtletaBId);

        if (atletaA is null || atletaB is null)
        {
            throw new InvalidOperationException("Atletas não encontrados");
        }

        var organizacao = await _organizacaoRepository.ObterPorId(atletaA.OrganizacaoId);
        if (organizacao is null)
        {
            throw new InvalidOperationException("Organização não encontrada");
        }

        var pontuacaoAntesA = atletaA.PontuacaoAtual;
        var pontuacaoAntesB = atletaB.PontuacaoAtual;

        int variacaoA;
        int variacaoB;
        string motivoA;
        string motivoB;

        switch (confronto.Resultado.Tipo)
        {
            case TipoResultado.VitoriaAtletaA:
                variacaoA = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoVitoria, 1);
                variacaoB = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoDerrota, -1);
                motivoA = $"Vitória sobre {atletaB.Nome.NomeFormatado}";
                motivoB = $"Derrota para {atletaA.Nome.NomeFormatado}";
                break;
            case TipoResultado.VitoriaAtletaB:
                variacaoA = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoDerrota, -1);
                variacaoB = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoVitoria, 1);
                motivoA = $"Derrota para {atletaB.Nome.NomeFormatado}";
                motivoB = $"Vitória sobre {atletaA.Nome.NomeFormatado}";
                break;
            case TipoResultado.Empate:
                variacaoA = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoEmpate, 1);
                variacaoB = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoEmpate, 1);
                motivoA = $"Empate com {atletaB.Nome.NomeFormatado}";
                motivoB = $"Empate com {atletaA.Nome.NomeFormatado}";
                break;
            case TipoResultado.WO:
                variacaoA = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoVitoria, 1);
                variacaoB = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoWO, -1);
                motivoA = $"Vitória por WO sobre {atletaB.Nome.NomeFormatado}";
                motivoB = $"Derrota por WO para {atletaA.Nome.NomeFormatado}";
                break;
            default:
                throw new InvalidOperationException("Tipo de resultado inválido");
        }

        var novaPontuacaoA = AplicarLimitesPontuacao(
            pontuacaoAntesA.Valor + variacaoA,
            organizacao.Configuracao.PontuacaoMinima,
            organizacao.Configuracao.PontuacaoMaxima);

        var novaPontuacaoB = AplicarLimitesPontuacao(
            pontuacaoAntesB.Valor + variacaoB,
            organizacao.Configuracao.PontuacaoMinima,
            organizacao.Configuracao.PontuacaoMaxima);

        atletaA.AtualizarPontuacao(novaPontuacaoA);
        atletaB.AtualizarPontuacao(novaPontuacaoB);

        await _atletaRepository.Atualizar(atletaA);
        await _atletaRepository.Atualizar(atletaB);

        var historicoA = new HistoricoRanking(
            atletaA.Id,
            atletaA.OrganizacaoId,
            pontuacaoAntesA,
            atletaA.PontuacaoAtual,
            confronto.Id,
            motivoA,
            usuarioId);

        var historicoB = new HistoricoRanking(
            atletaB.Id,
            atletaB.OrganizacaoId,
            pontuacaoAntesB,
            atletaB.PontuacaoAtual,
            confronto.Id,
            motivoB,
            usuarioId);

        await _historicoRepository.Adicionar(historicoA);
        await _historicoRepository.Adicionar(historicoB);
    }

    public async Task AplicarDecaimentoAtletasInativos()
    {
        var atletasInativos = await _atletaRepository.ObterAtletasSemConfrontoHaDias(30);

        foreach (var atleta in atletasInativos)
        {
            var organizacao = await _organizacaoRepository.ObterPorId(atleta.OrganizacaoId);
            if (organizacao is null)
            {
                continue;
            }

            var pontuacaoAntes = atleta.PontuacaoAtual;
            var variacaoDecaimento = AplicarVariacaoMinima(organizacao.Configuracao.PontuacaoDecaimentoSemanal, -1);
            var novaPontuacao = Math.Max(
                organizacao.Configuracao.PontuacaoPisoDecaimento,
                pontuacaoAntes.Valor + variacaoDecaimento);

            if (novaPontuacao == pontuacaoAntes.Valor)
            {
                continue;
            }

            atleta.AtualizarPontuacao(novaPontuacao);
            await _atletaRepository.Atualizar(atleta);

            var historico = new HistoricoRanking(
                atleta.Id,
                atleta.OrganizacaoId,
                pontuacaoAntes,
                atleta.PontuacaoAtual,
                null,
                "Decaimento por inatividade",
                SystemUsers.RankingDecayUserId);

            await _historicoRepository.Adicionar(historico);
        }
    }

    public async Task<IEnumerable<Atleta>> ObterRankingPorCategoria(Guid organizacaoId, Categoria? categoria = null, Genero? genero = null)
    {
        return await _atletaRepository.ObterRanking(organizacaoId, categoria, genero);
    }

    private static int AplicarVariacaoMinima(int variacaoConfigurada, int direcaoPadrao)
    {
        if (variacaoConfigurada > 0)
        {
            return Math.Max(1, variacaoConfigurada);
        }

        if (variacaoConfigurada < 0)
        {
            return Math.Min(-1, variacaoConfigurada);
        }

        return direcaoPadrao >= 0 ? 1 : -1;
    }

    private static int AplicarLimitesPontuacao(int valorCalculado, int minimo, int maximo)
    {
        return Math.Max(minimo, Math.Min(maximo, valorCalculado));
    }
}
