using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.ValueObjects;

public class Pontuacao : ValueObject
{
    public int Valor { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    private Pontuacao()
    {
        Valor = 0;
        DataAtualizacao = DateTime.UtcNow;
    }

    public Pontuacao(int valor)
    {
        if (valor < 0 || valor > 5000)
            throw new ArgumentException("Pontuação deve estar entre 0 e 5000", nameof(valor));

        Valor = valor;
        DataAtualizacao = DateTime.UtcNow;
    }

    public Pontuacao(int valor, DateTime dataAtualizacao)
    {
        if (valor < 0 || valor > 5000)
            throw new ArgumentException("Pontuação deve estar entre 0 e 5000", nameof(valor));

        Valor = valor;
        DataAtualizacao = dataAtualizacao;
    }

    public Pontuacao Atualizar(int novoValor)
    {
        return new Pontuacao(novoValor);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
        yield return DataAtualizacao;
    }

    public override string ToString() => $"{Valor} pontos";
}
