namespace AutorizacaoAutenticacao.Domain;

public sealed class ValorMonetarioInvalidoException : DomainException
{
    public ValorMonetarioInvalidoException(decimal montante)
        : base($"O montante '{montante}' é inválido: um valor monetário deve ser maior que zero.")
    {
    }
}
