## Why

O agregado `Pagamento` (Domain) já existe e é testado isoladamente, mas nenhuma camada superior consegue orquestrá-lo: não há ports para persistência nem para o contexto do chamador autenticado, e não existem casos de uso para consultar, criar ou cancelar pagamentos. Sem isso, a Application permanece vazia e a Api (Parte futura) não tem o que chamar.

## What Changes

- Adicionar `Id` (Guid) ao agregado `Pagamento` no Domain — pré-requisito mínimo para que um repositório consiga localizar e listar pagamentos individualmente. **BREAKING** para quem já construía `Pagamento` assumindo ausência de identidade (nenhum consumidor externo hoje).
- Definir os ports da Application: `IPagamentoRepository` (persistência do agregado `Pagamento`) e `ICallerContext` (identidade do chamador — `Subject`/`ClientId`, conforme documentado no README).
- Implementar os casos de uso `ConsultarPagamentos`, `CriarPagamento` e `CancelarPagamento`, orquestrando o Domain através dos ports, sem depender de infraestrutura real.
- Adicionar testes de aplicação usando fakes/in-memory dos ports (sem banco, sem HTTP, sem Keycloak).

## Capabilities

### New Capabilities
- `pagamento-aplicacao`: ports (`IPagamentoRepository`, `ICallerContext`) e casos de uso (`ConsultarPagamentos`, `CriarPagamento`, `CancelarPagamento`) que orquestram o agregado `Pagamento` sem depender de infraestrutura.

### Modified Capabilities
- `pagamento-dominio`: o agregado `Pagamento` passa a expor um `Id` (Guid) único, atribuído na criação, necessário para que a Application consiga localizar um pagamento específico.

## Impact

- **Domain** (`src/AutorizacaoAutenticacao.Domain`): `Pagamento.cs` ganha propriedade `Id`; `Domain.Tests` ganha cobertura para a nova invariante de identidade.
- **Application** (`src/AutorizacaoAutenticacao.Application`, hoje vazio): novos ports e casos de uso.
- **Application.Tests**: testes dos casos de uso com fakes in-memory dos ports.
- Nenhum impacto em Infrastructure ou Api nesta mudança — implementações reais dos ports (EF Core, `HttpContextAccessor`) ficam para uma parte futura.
