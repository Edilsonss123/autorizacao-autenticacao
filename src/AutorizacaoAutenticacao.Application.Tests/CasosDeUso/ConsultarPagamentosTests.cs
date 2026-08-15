using AutorizacaoAutenticacao.Application.CasosDeUso;
using AutorizacaoAutenticacao.Application.Tests.Fakes;
using AutorizacaoAutenticacao.Domain;

namespace AutorizacaoAutenticacao.Application.Tests.CasosDeUso;

public class ConsultarPagamentosTests
{
    [Fact]
    public async Task ExecutarAsync_ComPagamentosExistentes_RetornaTodosComIdValorEStatus()
    {
        var repositorio = new PagamentoRepositoryFake();
        var primeiro = Pagamento.Criar(10m);
        var segundo = Pagamento.Criar(20m);
        await repositorio.AdicionarAsync(primeiro, CancellationToken.None);
        await repositorio.AdicionarAsync(segundo, CancellationToken.None);
        var casoDeUso = new ConsultarPagamentos(repositorio);

        var pagamentos = await casoDeUso.ExecutarAsync(CancellationToken.None);

        Assert.Equal(2, pagamentos.Count);
        Assert.Contains(pagamentos, p => p.Id == primeiro.Id && p.Valor.Montante == 10m && p.Status == StatusPagamento.Pendente);
        Assert.Contains(pagamentos, p => p.Id == segundo.Id && p.Valor.Montante == 20m && p.Status == StatusPagamento.Pendente);
    }

    [Fact]
    public async Task ExecutarAsync_SemPagamentosExistentes_RetornaListaVazia()
    {
        var repositorio = new PagamentoRepositoryFake();
        var casoDeUso = new ConsultarPagamentos(repositorio);

        var pagamentos = await casoDeUso.ExecutarAsync(CancellationToken.None);

        Assert.Empty(pagamentos);
    }
}
