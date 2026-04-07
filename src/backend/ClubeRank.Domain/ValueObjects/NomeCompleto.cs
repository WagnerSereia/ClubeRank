using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.ValueObjects;

public class NomeCompleto : ValueObject
{
    public string PrimeiroNome { get; private set; }
    public string Sobrenome { get; private set; }

    private NomeCompleto()
    {
        PrimeiroNome = string.Empty;
        Sobrenome = string.Empty;
    }

    public NomeCompleto(string primeiroNome, string sobrenome)
    {
        if (string.IsNullOrWhiteSpace(primeiroNome))
            throw new ArgumentException("Primeiro nome não pode ser vazio", nameof(primeiroNome));
        if (string.IsNullOrWhiteSpace(sobrenome))
            throw new ArgumentException("Sobrenome não pode ser vazio", nameof(sobrenome));

        PrimeiroNome = primeiroNome.Trim();
        Sobrenome = sobrenome.Trim();
    }

    public string NomeFormatado => $"{PrimeiroNome} {Sobrenome}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PrimeiroNome;
        yield return Sobrenome;
    }

    public override string ToString() => NomeFormatado;
}
