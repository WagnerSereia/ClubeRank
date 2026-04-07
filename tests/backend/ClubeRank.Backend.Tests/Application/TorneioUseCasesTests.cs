using ClubeRank.Application.Commands;
using ClubeRank.Application.DTOs;
using ClubeRank.Application.UseCases;
using ClubeRank.Backend.Tests.TestDoubles;
using ClubeRank.Domain.Entities;
using ClubeRank.Domain.Services;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Backend.Tests.Application;

public class TorneioUseCasesTests
{
    [Fact]
    public async Task AdicionarAtletaTorneio_DeveBloquearAtletaDeOutraOrganizacao()
    {
        var organizacaoA = new Organizacao(
            "Clube A",
            new Email("a@cluberank.test"),
            null,
            "Tenis",
            TipoPlano.Basico);

        var organizacaoB = new Organizacao(
            "Clube B",
            new Email("b@cluberank.test"),
            null,
            "Tenis",
            TipoPlano.Basico);

        var torneio = new Torneio("Torneio A", TipoTorneio.Ladder, organizacaoA.Id, Categoria.A);
        var atletaOutroTenant = new Atleta(
            new NomeCompleto("Joao", "Outro"),
            Genero.Masculino,
            new Email("joao@cluberank.test"),
            null,
            Categoria.A,
            organizacaoB.Id);

        var torneioRepository = new FakeTorneioRepository();
        await torneioRepository.Adicionar(torneio);

        var atletaRepository = new FakeAtletaRepository();
        atletaRepository.Seed(atletaOutroTenant);

        var useCases = new TorneioUseCases(
            torneioRepository,
            new FakeConfrontoRepository(),
            atletaRepository,
            new ConfrontoService(new FakeConfrontoRepository(), atletaRepository),
            new RankingService(atletaRepository, new FakeHistoricoRankingRepository(), new FakeOrganizacaoRepository()));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCases.Handle(
                new AdicionarAtletaTorneioCommand(new AdicionarAtletaTorneioDto(torneio.Id, atletaOutroTenant.Id)),
                CancellationToken.None));
    }

    [Fact]
    public async Task GerarConfrontos_DeveCriarPareamentoLadder()
    {
        var organizacao = new Organizacao(
            "Clube A",
            new Email("org@cluberank.test"),
            null,
            "Tenis",
            TipoPlano.Basico);

        var atletaA = new Atleta(new NomeCompleto("Carlos", "Silva"), Genero.Masculino, new Email("carlos2@cluberank.test"), null, Categoria.A, organizacao.Id);
        var atletaB = new Atleta(new NomeCompleto("Bruno", "Souza"), Genero.Masculino, new Email("bruno2@cluberank.test"), null, Categoria.A, organizacao.Id);

        var torneio = new Torneio("Ladder", TipoTorneio.Ladder, organizacao.Id, Categoria.A);
        torneio.AdicionarAtleta(atletaA);
        torneio.AdicionarAtleta(atletaB);

        var torneioRepository = new FakeTorneioRepository();
        await torneioRepository.Adicionar(torneio);

        var atletaRepository = new FakeAtletaRepository();
        atletaRepository.Seed(atletaA, atletaB);

        var confrontoRepository = new FakeConfrontoRepository();
        var organizacaoRepository = new FakeOrganizacaoRepository();
        organizacaoRepository.Seed(organizacao);

        var useCases = new TorneioUseCases(
            torneioRepository,
            confrontoRepository,
            atletaRepository,
            new ConfrontoService(confrontoRepository, atletaRepository),
            new RankingService(atletaRepository, new FakeHistoricoRankingRepository(), organizacaoRepository));

        var confrontos = await useCases.Handle(new GerarConfrontosCommand(new GerarConfrontosDto(torneio.Id)), CancellationToken.None);
        var confronto = Assert.Single(confrontos);

        Assert.Equal(atletaA.Id, confronto.AtletaAId);
        Assert.Equal(atletaB.Id, confronto.AtletaBId);
        Assert.Equal(StatusConfronto.Agendado, confronto.Status);
        Assert.Single(confrontoRepository.Confrontos);
    }
}
