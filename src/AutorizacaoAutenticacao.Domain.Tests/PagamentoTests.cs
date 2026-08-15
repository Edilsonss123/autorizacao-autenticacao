namespace AutorizacaoAutenticacao.Domain.Tests;

public class PagamentoTests
{
    [Fact]
    public void Criar_ComMontanteValido_ResultaEmStatusPendente()
    {
        var pagamento = Pagamento.Criar(10m);

        Assert.Equal(StatusPagamento.Pendente, pagamento.Status);
        Assert.Equal(10m, pagamento.Valor.Montante);
    }

    [Fact]
    public void Criar_ComValorZero_LancaExcecaoDeDominio()
    {
        Assert.Throws<ValorMonetarioInvalidoException>(() => Pagamento.Criar(0m));
    }

    [Fact]
    public void Criar_ComValorNegativo_LancaExcecaoDeDominio()
    {
        Assert.Throws<ValorMonetarioInvalidoException>(() => Pagamento.Criar(-10m));
    }

    [Fact]
    public void Cancelar_ComStatusPendente_ResultaEmStatusCancelado()
    {
        var pagamento = Pagamento.Criar(10m);

        pagamento.Cancelar();

        Assert.Equal(StatusPagamento.Cancelado, pagamento.Status);
    }

    [Fact]
    public void Cancelar_ComStatusCancelado_LancaExcecaoDeDominioEMantemStatus()
    {
        var pagamento = Pagamento.Criar(10m);
        pagamento.Cancelar();

        Assert.Throws<PagamentoNaoPodeSerCanceladoException>(() => pagamento.Cancelar());
        Assert.Equal(StatusPagamento.Cancelado, pagamento.Status);
    }
}
