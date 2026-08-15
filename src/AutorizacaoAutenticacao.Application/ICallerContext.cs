namespace AutorizacaoAutenticacao.Application;

public interface ICallerContext
{
    string? Subject { get; }

    string? ClientId { get; }
}
