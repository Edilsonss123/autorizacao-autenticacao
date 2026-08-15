using AutorizacaoAutenticacao.Application.Portas;

namespace AutorizacaoAutenticacao.Application.Tests.Fakes;

public sealed class CallerContextFake : ICallerContext
{
    public string? Subject { get; set; }

    public string? ClientId { get; set; }
}
