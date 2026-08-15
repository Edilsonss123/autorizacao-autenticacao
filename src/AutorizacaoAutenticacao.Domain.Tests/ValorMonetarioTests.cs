using AutorizacaoAutenticacao.Domain.Excecoes;

namespace AutorizacaoAutenticacao.Domain.Tests;

public class ValorMonetarioTests
{
    [Fact]
    public void Criar_ComMontantePositivo_TemSucesso()
    {
        var valorMonetario = ValorMonetario.Criar(10m);

        Assert.Equal(10m, valorMonetario.Montante);
    }

    [Fact]
    public void Criar_ComMontanteZero_LancaExcecaoDeDominio()
    {
        Assert.Throws<ValorMonetarioInvalidoException>(() => ValorMonetario.Criar(0m));
    }

    [Fact]
    public void Criar_ComMontanteNegativo_LancaExcecaoDeDominio()
    {
        Assert.Throws<ValorMonetarioInvalidoException>(() => ValorMonetario.Criar(-10m));
    }

    [Fact]
    public void Equals_ComMesmoMontante_SaoIguais()
    {
        var primeiro = ValorMonetario.Criar(10m);
        var segundo = ValorMonetario.Criar(10m);

        Assert.Equal(primeiro, segundo);
        Assert.True(primeiro == segundo);
        Assert.False(primeiro != segundo);
        Assert.Equal(primeiro.GetHashCode(), segundo.GetHashCode());
    }

    [Fact]
    public void Equals_ComMontanteDiferente_NaoSaoIguais()
    {
        var primeiro = ValorMonetario.Criar(10m);
        var segundo = ValorMonetario.Criar(20m);

        Assert.NotEqual(primeiro, segundo);
        Assert.False(primeiro == segundo);
        Assert.True(primeiro != segundo);
    }

    [Fact]
    public void Equals_ComNull_RetornaFalso()
    {
        var valorMonetario = ValorMonetario.Criar(10m);

        Assert.False(valorMonetario.Equals(null));
        Assert.False(valorMonetario == null);
        Assert.False(null == valorMonetario);
        Assert.True(valorMonetario != null);
        Assert.True(null != valorMonetario);
    }

    [Fact]
    public void Equals_DoisNulls_OperadorRetornaTrue()
    {
        ValorMonetario? primeiro = null;
        ValorMonetario? segundo = null;

        Assert.True(primeiro == segundo);
        Assert.False(primeiro != segundo);
    }
}
