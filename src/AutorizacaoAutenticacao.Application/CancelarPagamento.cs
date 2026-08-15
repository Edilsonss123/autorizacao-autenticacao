namespace AutorizacaoAutenticacao.Application;

public sealed class CancelarPagamento(IPagamentoRepository pagamentoRepository)
{
    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await pagamentoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new PagamentoNaoEncontradoException(id);

        pagamento.Cancelar();

        await pagamentoRepository.AtualizarAsync(pagamento, cancellationToken);
    }
}
