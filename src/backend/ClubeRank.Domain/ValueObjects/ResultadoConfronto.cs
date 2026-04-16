using System.Text.Json.Serialization;

namespace ClubeRank.Domain.ValueObjects;

public class ResultadoConfronto : ValueObject
{
    public TipoResultado Tipo { get; private set; }
    public string? JustificativaWO { get; private set; }
    public DateTime DataRegistro { get; private set; }
    public List<SetConfronto> Sets { get; private set; } = [];

    [JsonIgnore]
    public int TotalSetsVencidosAtletaA => Sets.Count(x => x.AtletaAVenceu);

    [JsonIgnore]
    public int TotalSetsVencidosAtletaB => Sets.Count(x => x.AtletaBVenceu);

    [JsonIgnore]
    public int TotalGamesAtletaA => Sets.Sum(x => x.GamesAtletaA);

    [JsonIgnore]
    public int TotalGamesAtletaB => Sets.Sum(x => x.GamesAtletaB);

    private ResultadoConfronto()
    {
        Tipo = TipoResultado.VitoriaAtletaA;
        DataRegistro = DateTime.UtcNow;
    }

    public ResultadoConfronto(TipoResultado tipo, IEnumerable<SetConfronto>? sets = null, string? justificativaWO = null)
    {
        Tipo = tipo;
        DataRegistro = DateTime.UtcNow;
        Sets = sets?.OrderBy(x => x.Numero).ToList() ?? [];
        JustificativaWO = justificativaWO;

        ValidarConsistencia();
    }

    public static ResultadoConfronto CriarComSets(IEnumerable<SetConfronto> sets, string? justificativaWO = null)
    {
        var listaSets = sets?.OrderBy(x => x.Numero).ToList() ?? [];
        if (listaSets.Count == 0)
        {
            throw new ArgumentException("Ao menos um set deve ser informado para registrar o placar.", nameof(sets));
        }

        var setsAtletaA = listaSets.Count(x => x.AtletaAVenceu);
        var setsAtletaB = listaSets.Count(x => x.AtletaBVenceu);

        var tipo = setsAtletaA.CompareTo(setsAtletaB) switch
        {
            > 0 => TipoResultado.VitoriaAtletaA,
            < 0 => TipoResultado.VitoriaAtletaB,
            _ => TipoResultado.Empate
        };

        return new ResultadoConfronto(tipo, listaSets, justificativaWO);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tipo;
        yield return JustificativaWO;
        yield return DataRegistro;

        foreach (var set in Sets)
        {
            yield return set;
        }
    }

    private void ValidarConsistencia()
    {
        ValidarSets();

        if (Tipo == TipoResultado.WO)
        {
            if (string.IsNullOrWhiteSpace(JustificativaWO))
            {
                throw new ArgumentException("Justificativa e obrigatoria para WO.", nameof(JustificativaWO));
            }

            if (Sets.Count > 0)
            {
                throw new ArgumentException("Resultado por WO nao deve conter placar de sets.", nameof(Sets));
            }

            return;
        }

        if (Sets.Count == 0)
        {
            return;
        }

        var tipoInferido = TotalSetsVencidosAtletaA.CompareTo(TotalSetsVencidosAtletaB) switch
        {
            > 0 => TipoResultado.VitoriaAtletaA,
            < 0 => TipoResultado.VitoriaAtletaB,
            _ => TipoResultado.Empate
        };

        if (tipoInferido != Tipo)
        {
            throw new ArgumentException("O tipo de resultado nao corresponde ao placar informado.", nameof(Tipo));
        }
    }

    private void ValidarSets()
    {
        if (Sets.Count == 0)
        {
            return;
        }

        if (Sets.Any(x => x.Numero <= 0))
        {
            throw new ArgumentException("O numero do set deve ser maior que zero.", nameof(Sets));
        }

        if (Sets.Select(x => x.Numero).Distinct().Count() != Sets.Count)
        {
            throw new ArgumentException("Nao e permitido repetir a numeracao dos sets.", nameof(Sets));
        }
    }
}

public class SetConfronto : ValueObject
{
    public int Numero { get; private set; }
    public int GamesAtletaA { get; private set; }
    public int GamesAtletaB { get; private set; }
    public bool TieBreak { get; private set; }

    private SetConfronto()
    {
    }

    public SetConfronto(int numero, int gamesAtletaA, int gamesAtletaB, bool tieBreak = false)
    {
        if (numero <= 0)
        {
            throw new ArgumentException("O numero do set deve ser maior que zero.", nameof(numero));
        }

        if (gamesAtletaA < 0 || gamesAtletaB < 0)
        {
            throw new ArgumentException("O numero de games nao pode ser negativo.");
        }

        if (gamesAtletaA == gamesAtletaB)
        {
            throw new ArgumentException("Um set nao pode terminar empatado.");
        }

        Numero = numero;
        GamesAtletaA = gamesAtletaA;
        GamesAtletaB = gamesAtletaB;
        TieBreak = tieBreak;
    }

    public bool AtletaAVenceu => GamesAtletaA > GamesAtletaB;
    public bool AtletaBVenceu => GamesAtletaB > GamesAtletaA;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Numero;
        yield return GamesAtletaA;
        yield return GamesAtletaB;
        yield return TieBreak;
    }
}

public enum TipoResultado
{
    VitoriaAtletaA,
    VitoriaAtletaB,
    Empate,
    WO
}
