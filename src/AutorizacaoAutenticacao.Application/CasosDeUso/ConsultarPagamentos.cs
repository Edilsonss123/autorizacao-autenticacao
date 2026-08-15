using AutorizacaoAutenticacao.Application.Portas;
using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.CasosDeUso;

public sealed class ConsultarPagamentos(IPagamentoRepository pagamentoRepository)
{
    public Task<IReadOnlyCollection<Pagamento>> ExecutarAsync(CancellationToken cancellationToken) =>
        pagamentoRepository.ListarAsync(cancellationToken);
}
