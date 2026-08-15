## Context

Ver `proposal.md` para a motivação. Este documento cobre as decisões técnicas de como a solution e os projetos são estruturados, tomadas durante uma sessão de exploração com o usuário antes desta proposta.

## Goals / Non-Goals

**Goals:**
- Estabelecer a solution e os projetos vazios, com as referências corretas entre eles, de modo que a Arquitetura Hexagonal (`docs/arquitetura.md`) seja garantida estruturalmente (o compilador impede o Domain de depender de qualquer outra camada).
- Deixar o projeto pronto para receber o primeiro caso de uso real em uma próxima change, sem retrabalho de estrutura.

**Non-Goals:**
- Implementar qualquer entidade, caso de uso, endpoint, policy de autorização ou integração com Keycloak.
- Adicionar pacotes NuGet de terceiros (FluentValidation, cliente EF Core, etc.) — isso entra junto com o primeiro código real que os usa.
- Configurar CI/CD.

## Decisions

### Namespace raiz: `AutorizacaoAutenticacao`
Mais genérico que `Pagamentos` (nome do domínio usado só como exemplo no treinamento de Keycloak em `README.md`). Usa o mesmo nome do repositório, evitando acoplar o código a um domínio de negócio específico prematuramente.

### Nomes de camada em inglês (`Domain`, `Application`, `Infrastructure`, `Api`)
São termos consagrados do padrão Ports & Adapters, já usados assim em `docs/arquitetura.md`. `AGENTS.md` reserva português para o restante do código (classes, métodos, variáveis) — os nomes de projeto seguem o padrão arquitetural, não são "código de negócio".

### Formato `.slnx`
Novo formato de solution (XML) do SDK .NET, mais legível que o `.sln` clássico. O `README.md` já referencia .NET 10, que suporta esse formato nativamente via `dotnet sln`.

### Projeto único de teste de integração cobrindo `Infrastructure` + `Api`
Em vez de um `Api.Tests` separado, `AutorizacaoAutenticacao.Infrastructure.Tests` referencia tanto `Infrastructure` quanto `Api` e concentra os testes de integração (rodando contra os containers de `containers/`). Alternativa considerada: projeto de teste próprio para `Api`. Rejeitada porque, com Minimal API, o teste de um endpoint tende a já ser um teste de integração ponta a ponta (endpoint → caso de uso → adapter de infraestrutura), então separar os dois geraria duplicação de setup (containers, `WebApplicationFactory`) sem benefício claro nesta fase.

### Topologia de referências entre projetos
```
Domain
  ^
  |
Application
  ^
  |
Infrastructure --------+
  ^                    |
  |                    v
 Api  <-----------  (referencia Application e Infrastructure)

Domain.Tests            -> Domain
Application.Tests       -> Application
Infrastructure.Tests    -> Infrastructure, Api
```
Nenhum projeto referencia `Api` além dos seus próprios testes — `Api` é a camada mais externa (adapter de entrada), nada depende dela.

## Risks / Trade-offs

- **Nomes em inglês para as camadas vs. regra "código sempre em português"** → Mitigação: registrado explicitamente aqui e em `docs/arquitetura.md` como exceção deliberada (termo consagrado), evitando ambiguidade em changes futuras.
- **Projeto de teste único para Infrastructure+Api pode crescer demais** → Mitigação: se a suíte ficar grande/lenta, separar em uma change futura é um refactor mecânico (mover arquivos, criar novo `.csproj`), não uma mudança de comportamento.
- **`.slnx` é um formato relativamente novo** → Mitigação: é suportado nativamente pelo SDK .NET usado no projeto; caso cause atrito com alguma ferramenta, converter para `.sln` é trivial (`dotnet sln` opera sobre ambos).
