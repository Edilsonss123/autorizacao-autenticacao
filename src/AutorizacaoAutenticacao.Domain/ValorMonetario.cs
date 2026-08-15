using AutorizacaoAutenticacao.Domain.Excecoes;

namespace AutorizacaoAutenticacao.Domain;

// Não é um record: `with` bypassaria a validação do construtor (ver design.md).
public sealed class ValorMonetario : IEquatable<ValorMonetario>
{
    public decimal Montante { get; }

    private ValorMonetario(decimal montante)
    {
        Montante = montante;
    }

    public static ValorMonetario Criar(decimal montante)
    {
        if (montante <= 0)
        {
            throw new ValorMonetarioInvalidoException(montante);
        }

        return new ValorMonetario(montante);
    }

    public bool Equals(ValorMonetario? other)
    {
        if (other is null)
        {
            return false;
        }

        return Montante == other.Montante;
    }

    public override bool Equals(object? obj) => Equals(obj as ValorMonetario);

    public override int GetHashCode() => Montante.GetHashCode();

    public static bool operator ==(ValorMonetario? esquerda, ValorMonetario? direita)
    {
        if (esquerda is null)
        {
            return direita is null;
        }

        return esquerda.Equals(direita);
    }

    public static bool operator !=(ValorMonetario? esquerda, ValorMonetario? direita) => !(esquerda == direita);
}
