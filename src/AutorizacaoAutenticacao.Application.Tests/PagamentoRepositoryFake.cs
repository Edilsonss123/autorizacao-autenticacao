using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.Tests;

public sealed class PagamentoRepositoryFake : IPagamentoRepository
{
    private readonly Dictionary<Guid, Pagamento> _pagamentos = [];
    private readonly List<Guid> _idsAtualizados = [];

    public IReadOnlyList<Guid> IdsAtualizados => _idsAtualizados;

    public Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken)
    {
        _pagamentos[pagamento.Id] = pagamento;
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken)
    {
        _idsAtualizados.Add(pagamento.Id);
        _pagamentos[pagamento.Id] = pagamento;
        return Task.CompletedTask;
    }

    public Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _pagamentos.TryGetValue(id, out var pagamento);
        return Task.FromResult(pagamento);
    }

    public Task<IReadOnlyCollection<Pagamento>> ListarAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Pagamento> pagamentos = _pagamentos.Values.ToList();
        return Task.FromResult(pagamentos);
    }
}
