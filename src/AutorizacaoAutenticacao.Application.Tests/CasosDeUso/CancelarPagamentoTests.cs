using AutorizacaoAutenticacao.Application.CasosDeUso;
using AutorizacaoAutenticacao.Application.Excecoes;
using AutorizacaoAutenticacao.Application.Tests.Fakes;
using AutorizacaoAutenticacao.Domain;
using AutorizacaoAutenticacao.Domain.Excecoes;

namespace AutorizacaoAutenticacao.Application.Tests.CasosDeUso;

public class CancelarPagamentoTests
{
    [Fact]
    public async Task ExecutarAsync_ComPagamentoPendenteExistente_PersisteStatusCancelado()
    {
        var repositorio = new PagamentoRepositoryFake();
        var pagamento = Pagamento.Criar(10m);
        await repositorio.AdicionarAsync(pagamento, CancellationToken.None);
        var casoDeUso = new CancelarPagamento(repositorio);

        await casoDeUso.ExecutarAsync(pagamento.Id, CancellationToken.None);

        Assert.Contains(pagamento.Id, repositorio.IdsAtualizados);
        var persistido = await repositorio.ObterPorIdAsync(pagamento.Id, CancellationToken.None);
        Assert.NotNull(persistido);
        Assert.Equal(StatusPagamento.Cancelado, persistido!.Status);
    }

    [Fact]
    public async Task ExecutarAsync_ComIdInexistente_LancaPagamentoNaoEncontradoENaoAlteraNada()
    {
        var repositorio = new PagamentoRepositoryFake();
        var casoDeUso = new CancelarPagamento(repositorio);
        var idInexistente = Guid.NewGuid();

        await Assert.ThrowsAsync<PagamentoNaoEncontradoException>(
            () => casoDeUso.ExecutarAsync(idInexistente, CancellationToken.None));

        var pagamentos = await repositorio.ListarAsync(CancellationToken.None);
        Assert.Empty(pagamentos);
    }

    [Fact]
    public async Task ExecutarAsync_ComPagamentoNaoPendente_LancaExcecaoDeDominioEMantemStatus()
    {
        var repositorio = new PagamentoRepositoryFake();
        var pagamento = Pagamento.Criar(10m);
        pagamento.Cancelar();
        await repositorio.AdicionarAsync(pagamento, CancellationToken.None);
        var casoDeUso = new CancelarPagamento(repositorio);

        await Assert.ThrowsAsync<PagamentoNaoPodeSerCanceladoException>(
            () => casoDeUso.ExecutarAsync(pagamento.Id, CancellationToken.None));

        Assert.DoesNotContain(pagamento.Id, repositorio.IdsAtualizados);
        var persistido = await repositorio.ObterPorIdAsync(pagamento.Id, CancellationToken.None);
        Assert.Equal(StatusPagamento.Cancelado, persistido!.Status);
    }
}
