namespace AutorizacaoAutenticacao.Application;

public sealed class PagamentoNaoEncontradoException : Exception
{
    public PagamentoNaoEncontradoException(Guid id)
        : base($"Nenhum pagamento encontrado com o Id '{id}'.")
    {
    }
}
