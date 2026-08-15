using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AutorizacaoAutenticacao.Infrastructure.Tests;

public class InicializacaoApiTestes
{
    [Fact]
    public async Task DeveSubirOHostHttpSemFalhar()
    {
        await using var aplicacao = new WebApplicationFactory<Program>();
        using var cliente = aplicacao.CreateClient();

        var resposta = await cliente.GetAsync("/");

        Assert.NotEqual(HttpStatusCode.InternalServerError, resposta.StatusCode);
    }
}
