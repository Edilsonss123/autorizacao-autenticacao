namespace AutorizacaoAutenticacao.Application.Portas;

public interface ICallerContext
{
    string? Subject { get; }

    string? ClientId { get; }
}
