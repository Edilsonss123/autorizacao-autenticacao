## 1. Pré-requisitos

- [x] 1.1 Confirmar que o SDK do .NET instalado suporta `net10.0` e o formato de solution `.slnx` (`dotnet --version`)

## 2. Solution

- [x] 2.1 Criar a pasta `src/` na raiz do repositório
- [x] 2.2 Criar a solution `AutorizacaoAutenticacao.slnx` em `src/`, no formato `.slnx` (`dotnet new sln --format slnx -n AutorizacaoAutenticacao -o src`; se o SDK não aceitar `--format slnx` diretamente, criar como `.sln` e converter com `dotnet sln migrate`)

## 3. Projetos de código (Domain, Application, Infrastructure, Api)

- [x] 3.1 Criar `AutorizacaoAutenticacao.Domain` como class library (`dotnet new classlib`), target `net10.0`, sem nenhuma classe de exemplo (remover `Class1.cs`)
- [x] 3.2 Criar `AutorizacaoAutenticacao.Application` como class library, target `net10.0`, sem classe de exemplo
- [x] 3.3 Criar `AutorizacaoAutenticacao.Infrastructure` como class library, target `net10.0`, sem classe de exemplo
- [x] 3.4 Criar `AutorizacaoAutenticacao.Api` como projeto web Minimal API (`dotnet new web`), target `net10.0`, com `Program.cs` reduzido ao mínimo (`WebApplication.CreateBuilder` → `Build` → `Run`, sem os endpoints de exemplo do template)

## 4. Projetos de teste (xUnit)

- [x] 4.1 Criar `AutorizacaoAutenticacao.Domain.Tests` (`dotnet new xunit`), sem o teste de exemplo (`UnitTest1.cs`)
- [x] 4.2 Criar `AutorizacaoAutenticacao.Application.Tests` (`dotnet new xunit`), sem o teste de exemplo
- [x] 4.3 Criar `AutorizacaoAutenticacao.Infrastructure.Tests` (`dotnet new xunit`), sem o teste de exemplo

## 5. Referências entre projetos

- [x] 5.1 `Application` referencia `Domain`
- [x] 5.2 `Infrastructure` referencia `Application`
- [x] 5.3 `Api` referencia `Application` e `Infrastructure`
- [x] 5.4 `Domain.Tests` referencia `Domain`
- [x] 5.5 `Application.Tests` referencia `Application`
- [x] 5.6 `Infrastructure.Tests` referencia `Infrastructure` e `Api`
- [x] 5.7 Confirmar que `Domain` não referencia nenhum outro projeto (garantia estrutural da regra "Domain não depende de nada")

## 6. Solution final

- [x] 6.1 Adicionar todos os 7 projetos (`Domain`, `Application`, `Infrastructure`, `Api`, `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`) à `AutorizacaoAutenticacao.slnx`

## 7. Validação

- [x] 7.1 `dotnet build` na solution completa sem erros
- [x] 7.2 `dotnet test` na solution executa sem erros (projetos de teste vazios, nenhum teste a rodar ainda)
