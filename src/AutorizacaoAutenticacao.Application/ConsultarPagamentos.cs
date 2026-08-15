using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application;

public sealed class ConsultarPagamentos(IPagamentoRepository pagamentoRepository)
{
    public Task<IReadOnlyCollection<Pagamento>> ExecutarAsync(CancellationToken cancellationToken) =>
        pagamentoRepository.ListarAsync(cancellationToken);
}
