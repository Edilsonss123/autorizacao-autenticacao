using AutorizacaoAutenticacao.Application.Portas;
using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.CasosDeUso;

public sealed class CriarPagamento(IPagamentoRepository pagamentoRepository)
{
    public async Task<Pagamento> ExecutarAsync(decimal montante, CancellationToken cancellationToken)
    {
        var pagamento = Pagamento.Criar(montante);

        await pagamentoRepository.AdicionarAsync(pagamento, cancellationToken);

        return pagamento;
    }
}
