namespace AutorizacaoAutenticacao.Domain.Excecoes;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
