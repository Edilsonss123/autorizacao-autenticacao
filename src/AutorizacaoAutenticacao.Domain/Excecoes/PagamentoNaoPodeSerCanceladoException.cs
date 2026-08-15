namespace AutorizacaoAutenticacao.Domain.Excecoes;

public sealed class PagamentoNaoPodeSerCanceladoException : DomainException
{
    public PagamentoNaoPodeSerCanceladoException(StatusPagamento statusAtual)
        : base($"Não é possível cancelar um pagamento com status '{statusAtual}': só é permitido cancelar pagamentos com status '{StatusPagamento.Pendente}'.")
    {
    }
}
