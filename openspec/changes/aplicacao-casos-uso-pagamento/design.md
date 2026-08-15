## Context

Ver `proposal.md` para motivação. Estado atual: `Domain` tem `Pagamento`, `ValorMonetario`, `StatusPagamento` e exceções de domínio, sem `Id`. `Application` e `Application.Tests` estão vazios (apenas scaffold). O README (seção 43) já documenta o contrato esperado de `ICallerContext` (`Subject`, `ClientId`), e `docs/arquitetura.md` fixa a regra de dependência `Infrastructure -> Application -> Domain`.

## Goals / Non-Goals

**Goals:**
- Ports mínimos e estáveis (`IPagamentoRepository`, `ICallerContext`) que a Infrastructure poderá implementar em uma parte futura sem precisar mudar a Application.
- Três casos de uso (`CriarPagamento`, `CancelarPagamento`, `ConsultarPagamentos`) cobertos por TDD, usando apenas fakes/in-memory dos ports.
- Adicionar o mínimo necessário ao Domain (`Id`) para viabilizar os casos de uso, sem antecipar decisões que pertencem a partes futuras.

**Non-Goals:**
- Implementações reais dos ports (EF Core, `HttpContextAccessor`) — ficam para a parte de Infrastructure.
- Autorização por `EmpresaId` (`pagamento.EmpresaId == usuario.EmpresaId`) — decisão explícita do usuário de adiar essa regra; `ICallerContext` fica minimalista (`Subject`/`ClientId`), como no README.
- Endpoints HTTP (Minimal API) — ficam para a parte de Api.
- Persistência real ou concorrência/transações — os fakes in-memory não precisam ser thread-safe.

## Decisions

**Id do Pagamento gerado no próprio Domain, não na Application.** `Pagamento.Criar` passa a gerar um `Guid.NewGuid()` internamente. Alternativa considerada: gerar o `Id` na Application (caso de uso) e passar para o Domain — rejeitada porque violaria a regra de que o Domain garante suas próprias invariantes (aqui, identidade única) independentemente de quem o chama.

**`IPagamentoRepository` com quatro operações: `Adicionar`, `Atualizar`, `ObterPorId`, `Listar`.** Cobrem exatamente os três casos de uso desta parte; `ObterPorId` retorna um `Pagamento?` (null quando não encontrado) em vez de lançar exceção, mantendo a decisão de "não encontrado" como responsabilidade do caso de uso (que decide o erro a retornar ao chamador), não do repositório. `Atualizar` existe separado de `Adicionar` porque `CancelarPagamento` precisa persistir a transição de estado explicitamente através do port — sem essa chamada, a spec ("persistido através do port de persistência") só seria satisfeita por acidente, porque o fake in-memory guarda a mesma referência de objeto que `Cancelar()` mutou; um adapter real (EF Core sem tracking, HTTP) perderia a alteração silenciosamente.

**"Não encontrado" em `CancelarPagamento` é uma exceção de aplicação, não de domínio.** Como o Domain não tem conceito de "pagamento inexistente" (ele só existe se foi criado), essa é uma regra da Application. Será representada por uma exceção própria da Application (ex.: `PagamentoNaoEncontradoException`), distinta das exceções de domínio já existentes, para manter a mesma convenção de "sem infraestrutura real, mas com tipos de erro explícitos e testáveis".

**`ICallerContext` definido mas não usado nas regras de negócio desta parte.** O port é definido porque está listado explicitamente no escopo da Parte 9 e porque casos de uso futuros (auditoria, autorização por `EmpresaId`) vão precisar dele — mas nenhum caso de uso desta parte aplica lógica condicionada a `Subject`/`ClientId` ainda, evitando implementar regra de negócio que não está especificada.

**Casos de uso como classes com um método de execução (`ExecutarAsync`), seguindo o exemplo de `AGENTS.md`.** Alternativa (handlers via MediatR ou biblioteca de mediator) rejeitada por adicionar uma dependência não solicitada nem necessária para três casos de uso simples.

## Risks / Trade-offs

- [`Id` gerado com `Guid.NewGuid()` em vez de um identificador ordenável (ex.: `Guid v7`)] → aceitável nesta fase; pode ser revisitado quando a Infrastructure (persistência real) for definida, sem quebrar a spec (a spec exige apenas "Guid único", não uma estratégia de geração específica).
- [Fakes in-memory dos testes de Application podem divergir do comportamento real do futuro adapter EF Core (ex.: ordenação, concorrência)] → mitigado por manter os fakes simples (dicionário em memória) e deixar validação de comportamento real para os testes de integração da Infrastructure, como já determina `AGENTS.md`.
- [Adicionar `Id` ao `Pagamento` é uma mudança de forma no agregado já arquivado em `pagamento-dominio`] → mitigado por ser aditivo (nova propriedade, nenhuma quebra dos requisitos e cenários já existentes) e coberto por delta spec (`ADDED Requirements`) nesta mudança.
