using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;
using ClubeRank.Domain.Interfaces;

namespace ClubeRank.Domain.Services;

public interface IConfrontoService
{
    Task<IEnumerable<Confronto>> GerarConfrontosLadder(Torneio torneio);
    Task<IEnumerable<Confronto>> GerarConfrontosRoundRobin(Torneio torneio);
    Task<bool> PodeGerarConfronto(Atleta atletaA, Atleta atletaB, Torneio torneio);
}

public class ConfrontoService : IConfrontoService
{
    private readonly IConfrontoRepository _confrontoRepository;
    private readonly IAtletaRepository _atletaRepository;

    public ConfrontoService(IConfrontoRepository confrontoRepository, IAtletaRepository atletaRepository)
    {
        _confrontoRepository = confrontoRepository;
        _atletaRepository = atletaRepository;
    }

    public async Task<IEnumerable<Confronto>> GerarConfrontosLadder(Torneio torneio)
    {
        if (torneio.Atletas.Count < 2)
            throw new InvalidOperationException("Torneio deve ter pelo menos 2 atletas");

        var atletas = torneio.Atletas.OrderByDescending(a => a.PontuacaoAtual.Valor).ToList();
        var confrontos = new List<Confronto>();

        // Algoritmo ladder: parear atletas próximos no ranking
        for (int i = 0; i < atletas.Count - 1; i += 2)
        {
            var atletaA = atletas[i];
            var atletaB = atletas[i + 1];

            if (await PodeGerarConfronto(atletaA, atletaB, torneio))
            {
                var confronto = new Confronto(
                    atletaA.Id,
                    atletaB.Id,
                    torneio.Id,
                    torneio.OrganizacaoId,
                    DateTime.UtcNow.AddDays(1)); // Agendar para amanhã

                confrontos.Add(confronto);
            }
        }

        return confrontos;
    }

    public async Task<IEnumerable<Confronto>> GerarConfrontosRoundRobin(Torneio torneio)
    {
        if (torneio.Atletas.Count < 2)
            throw new InvalidOperationException("Torneio deve ter pelo menos 2 atletas");

        var atletas = torneio.Atletas.ToList();
        var confrontos = new List<Confronto>();

        // Algoritmo round-robin simples: todos contra todos
        for (int i = 0; i < atletas.Count; i++)
        {
            for (int j = i + 1; j < atletas.Count; j++)
            {
                var atletaA = atletas[i];
                var atletaB = atletas[j];

                if (await PodeGerarConfronto(atletaA, atletaB, torneio))
                {
                    var confronto = new Confronto(
                        atletaA.Id,
                        atletaB.Id,
                        torneio.Id,
                        torneio.OrganizacaoId,
                        DateTime.UtcNow.AddDays(1 + (i + j))); // Distribuir datas

                    confrontos.Add(confronto);
                }
            }
        }

        return confrontos;
    }

    public async Task<bool> PodeGerarConfronto(Atleta atletaA, Atleta atletaB, Torneio torneio)
    {
        // Verificar se atletas estão ativos
        if (atletaA.Status != StatusAtleta.Ativo || atletaB.Status != StatusAtleta.Ativo)
            return false;

        // Verificar se já existe confronto agendado entre eles no torneio
        var confrontosExistentes = await _confrontoRepository.ObterConfrontosEntreAtletasNoTorneio(
            atletaA.Id, atletaB.Id, torneio.Id);

        if (confrontosExistentes.Any(c => c.Status == StatusConfronto.Agendado))
            return false;

        // Verificar se algum dos atletas já tem confronto no mesmo dia
        var dataPretendida = DateTime.UtcNow.AddDays(1).Date;
        var confrontosAtletaA = await _confrontoRepository.ObterConfrontosDoAtletaNaData(atletaA.Id, dataPretendida);
        var confrontosAtletaB = await _confrontoRepository.ObterConfrontosDoAtletaNaData(atletaB.Id, dataPretendida);

        if (confrontosAtletaA.Any() || confrontosAtletaB.Any())
            return false;

        // Verificar histórico recente de confrontos (evitar repetição)
        var confrontosRecentes = await _confrontoRepository.ObterHistoricoConfrontosEntreAtletas(
            atletaA.Id, atletaB.Id, 10); // Últimos 10 confrontos

        // Se tiveram confronto recentemente, verificar se há alternativas melhores
        if (confrontosRecentes.Any())
        {
            // Lógica para priorizar adversários nunca enfrentados
            // Por simplicidade, permitir apenas se não houver outras opções
            var outrosAtletas = torneio.Atletas.Where(a => a.Id != atletaA.Id && a.Id != atletaB.Id).ToList();
            var haAlternativas = false;

            foreach (var outro in outrosAtletas)
            {
                var confrontosComOutro = await _confrontoRepository.ObterHistoricoConfrontosEntreAtletas(
                    atletaA.Id, outro.Id, 1);

                if (!confrontosComOutro.Any() && await PodeGerarConfronto(atletaA, outro, torneio))
                {
                    haAlternativas = true;
                    break;
                }
            }

            if (haAlternativas)
                return false; // Preferir adversários nunca enfrentados
        }

        return true;
    }
}

// Interfaces de repositório
// Movidas para ClubeRank.Domain.Interfaces.IRepositories
