using AutorizacaoAutenticacao.Domain.Excecoes;

namespace AutorizacaoAutenticacao.Domain;

public sealed class Pagamento
{
    public Guid Id { get; }

    public ValorMonetario Valor { get; }

    public StatusPagamento Status { get; private set; }

    private Pagamento(ValorMonetario valor)
    {
        Id = Guid.NewGuid();
        Valor = valor;
        Status = StatusPagamento.Pendente;
    }

    public static Pagamento Criar(decimal montante)
    {
        var valor = ValorMonetario.Criar(montante);
        return new Pagamento(valor);
    }

    public void Cancelar()
    {
        if (Status != StatusPagamento.Pendente)
        {
            throw new PagamentoNaoPodeSerCanceladoException(Status);
        }

        Status = StatusPagamento.Cancelado;
    }
}
