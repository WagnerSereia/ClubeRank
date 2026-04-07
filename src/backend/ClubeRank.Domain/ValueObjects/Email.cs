using System.Text.RegularExpressions;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Valor { get; private set; }

    private Email()
    {
        Valor = string.Empty;
    }

    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Email com formato inválido", nameof(email));

        Valor = email.ToLower().Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
