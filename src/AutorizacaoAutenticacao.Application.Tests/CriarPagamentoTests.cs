using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.Tests;

public class CriarPagamentoTests
{
    [Fact]
    public async Task ExecutarAsync_ComValorValido_PersisteERetornaPagamentoPendente()
    {
        var repositorio = new PagamentoRepositoryFake();
        var casoDeUso = new CriarPagamento(repositorio);

        var pagamento = await casoDeUso.ExecutarAsync(10m, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, pagamento.Id);
        Assert.Equal(StatusPagamento.Pendente, pagamento.Status);

        var persistido = await repositorio.ObterPorIdAsync(pagamento.Id, CancellationToken.None);
        Assert.NotNull(persistido);
        Assert.Equal(pagamento.Id, persistido!.Id);
    }

    [Fact]
    public async Task ExecutarAsync_ComValorInvalido_LancaExcecaoDeDominioENaoPersisteNada()
    {
        var repositorio = new PagamentoRepositoryFake();
        var casoDeUso = new CriarPagamento(repositorio);

        await Assert.ThrowsAsync<ValorMonetarioInvalidoException>(
            () => casoDeUso.ExecutarAsync(0m, CancellationToken.None));

        var pagamentos = await repositorio.ListarAsync(CancellationToken.None);
        Assert.Empty(pagamentos);
    }
}
