using AutorizacaoAutenticacao.Application.Excecoes;
using AutorizacaoAutenticacao.Application.Portas;

namespace AutorizacaoAutenticacao.Application.CasosDeUso;

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
