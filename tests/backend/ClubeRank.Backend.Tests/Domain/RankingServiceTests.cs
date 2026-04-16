using ClubeRank.Backend.Tests.TestDoubles;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Backend.Tests.Domain;

public class RankingServiceTests
{
    [Fact]
    public async Task AtualizarRankingAposConfronto_DeveAtualizarPontuacaoERegistrarHistorico()
    {
        var organizacao = new Organizacao(
            "Clube A",
            new Email("org@cluberank.test"),
            "11999999999",
            "Tenis",
            TipoPlano.Basico);

        var atletaA = new Atleta(
            new NomeCompleto("Carlos", "Silva"),
            Genero.Masculino,
            new Email("carlos@cluberank.test"),
            "11911111111",
            Categoria.A,
            organizacao.Id);

        var atletaB = new Atleta(
            new NomeCompleto("Bruno", "Souza"),
            Genero.Masculino,
            new Email("bruno@cluberank.test"),
            "11922222222",
            Categoria.A,
            organizacao.Id);

        var confronto = new Confronto(atletaA.Id, atletaB.Id, Guid.NewGuid(), organizacao.Id, DateTime.UtcNow.AddDays(1));
        confronto.RegistrarResultado(new ResultadoConfronto(TipoResultado.VitoriaAtletaA));

        var atletaRepository = new FakeAtletaRepository();
        atletaRepository.Seed(atletaA, atletaB);

        var historicoRepository = new FakeHistoricoRankingRepository();
        var organizacaoRepository = new FakeOrganizacaoRepository();
        organizacaoRepository.Seed(organizacao);

        var service = new RankingService(atletaRepository, historicoRepository, organizacaoRepository);

        await service.AtualizarRankingAposConfronto(confronto, Guid.NewGuid());

        Assert.Equal(1010, atletaA.PontuacaoAtual.Valor);
        Assert.Equal(995, atletaB.PontuacaoAtual.Valor);
        Assert.Equal(2, historicoRepository.Historicos.Count);
        Assert.All(historicoRepository.Historicos, historico => Assert.Equal(confronto.Id, historico.ConfrontoId));
    }

    [Fact]
    public async Task AtualizarRankingAposConfronto_ComSets_DeveAplicarPontuacaoBaseEMaisPontosPorSet()
    {
        var organizacao = new Organizacao(
            "Clube A",
            new Email("org-set@cluberank.test"),
            "11999999999",
            "Tenis",
            TipoPlano.Basico);

        var atletaA = new Atleta(
            new NomeCompleto("Wagner", "Sereia"),
            Genero.Masculino,
            new Email("wagner@cluberank.test"),
            "11911111111",
            Categoria.A,
            organizacao.Id);

        var atletaB = new Atleta(
            new NomeCompleto("Ulisses", "Costa"),
            Genero.Masculino,
            new Email("ulisses@cluberank.test"),
            "11922222222",
            Categoria.A,
            organizacao.Id);

        var torneio = new Torneio("Melhor de 3", TipoTorneio.Ladder, organizacao.Id, Categoria.A);
        torneio.AtualizarConfiguracaoPontuacao(10, -5, 0, -20, 2, 3, false);

        var confronto = new Confronto(atletaA.Id, atletaB.Id, torneio.Id, organizacao.Id, DateTime.UtcNow.AddDays(1));
        typeof(Confronto).GetProperty(nameof(Confronto.Torneio))!.SetValue(confronto, torneio);
        confronto.RegistrarResultado(ResultadoConfronto.CriarComSets(
        [
            new SetConfronto(1, 6, 4),
            new SetConfronto(2, 3, 6),
            new SetConfronto(3, 10, 8, true)
        ]));

        var atletaRepository = new FakeAtletaRepository();
        atletaRepository.Seed(atletaA, atletaB);

        var historicoRepository = new FakeHistoricoRankingRepository();
        var organizacaoRepository = new FakeOrganizacaoRepository();
        organizacaoRepository.Seed(organizacao);

        var service = new RankingService(atletaRepository, historicoRepository, organizacaoRepository);

        await service.AtualizarRankingAposConfronto(confronto, Guid.NewGuid());

        Assert.Equal(1014, atletaA.PontuacaoAtual.Valor);
        Assert.Equal(997, atletaB.PontuacaoAtual.Valor);
    }
}
