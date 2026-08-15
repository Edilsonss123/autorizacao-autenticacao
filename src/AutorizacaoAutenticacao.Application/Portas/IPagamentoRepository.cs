using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.Portas;

public interface IPagamentoRepository
{
    Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken);

    Task AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken);

    Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Pagamento>> ListarAsync(CancellationToken cancellationToken);
}
