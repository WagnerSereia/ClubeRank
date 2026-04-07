using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.ValueObjects;

public class ResultadoConfronto : ValueObject
{
    public TipoResultado Tipo { get; private set; }
    public string? JustificativaWO { get; private set; }
    public DateTime DataRegistro { get; private set; }

    private ResultadoConfronto()
    {
        Tipo = TipoResultado.VitoriaAtletaA;
        DataRegistro = DateTime.UtcNow;
    }

    public ResultadoConfronto(TipoResultado tipo, string? justificativaWO = null)
    {
        Tipo = tipo;
        DataRegistro = DateTime.UtcNow;

        if (tipo == TipoResultado.WO && string.IsNullOrWhiteSpace(justificativaWO))
            throw new ArgumentException("Justificativa é obrigatória para WO", nameof(justificativaWO));

        JustificativaWO = justificativaWO;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tipo;
        yield return JustificativaWO;
        yield return DataRegistro;
    }
}

public enum TipoResultado
{
    VitoriaAtletaA,
    VitoriaAtletaB,
    Empate,
    WO
}
