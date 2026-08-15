namespace AutorizacaoAutenticacao.Application.Tests;

public sealed class CallerContextFake : ICallerContext
{
    public string? Subject { get; set; }

    public string? ClientId { get; set; }
}
