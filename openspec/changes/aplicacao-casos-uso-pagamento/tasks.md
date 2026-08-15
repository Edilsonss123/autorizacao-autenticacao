## 1. Preparação

- [x] 1.1 Validar se já existe uma branch dedicada para esta mudança; se não existir, criar uma nova branch a partir da `main` antes de implementar qualquer coisa (nunca commitar direto na `main`)
- [x] 1.2 Confirmar/abrir a issue no GitHub correspondente a esta mudança (fluxo issue-driven), vinculando-a ao PR que será aberto ao final

## 2. Domain — Identificação do Pagamento (TDD)

- [x] 2.1 Escrever teste em `Domain.Tests` garantindo que `Pagamento.Criar` atribui um `Id` (`Guid`) diferente de `Guid.Empty`
- [x] 2.2 Escrever teste garantindo que dois `Pagamento` criados em chamadas separadas possuem `Id` distintos entre si
- [x] 2.3 Implementar a propriedade `Id` em `Pagamento` (Domain), gerada em `Criar`, até os testes passarem
- [x] 2.4 Rodar a suíte de testes de `Domain.Tests` e confirmar que os requisitos já existentes (criação, cancelamento, valor inválido) continuam passando sem alteração

## 3. Application — Ports e fakes de teste

- [x] 3.1 Definir `IPagamentoRepository` em `Application` com as operações necessárias aos casos de uso (adicionar, obter por `Id`, listar todos)
- [x] 3.2 Definir `ICallerContext` em `Application` expondo `Subject` e `ClientId` do chamador autenticado
- [x] 3.3 Implementar um fake/in-memory de `IPagamentoRepository` em `Application.Tests`, sem dependência de banco de dados
- [x] 3.4 Implementar um fake de `ICallerContext` em `Application.Tests`

## 4. Application — Caso de uso CriarPagamento (TDD)

- [x] 4.1 Escrever teste do caso de uso `CriarPagamento` com valor monetário válido, usando o repositório fake, verificando que o `Pagamento` retornado tem `Id` e `Status = Pendente` e foi persistido
- [x] 4.2 Escrever teste do caso de uso `CriarPagamento` com valor monetário zero/negativo, verificando que a exceção de domínio é propagada e nada é persistido
- [x] 4.3 Implementar `CriarPagamento` orquestrando `Pagamento.Criar` e `IPagamentoRepository` até os testes passarem

## 5. Application — Caso de uso CancelarPagamento (TDD)

- [x] 5.1 Escrever teste do caso de uso `CancelarPagamento` com o `Id` de um `Pagamento` pendente existente, verificando que o `Status` persistido passa a `Cancelado`
- [x] 5.2 Escrever teste do caso de uso `CancelarPagamento` com um `Id` inexistente, verificando que um erro de "não encontrado" é retornado e nada é persistido
- [x] 5.3 Escrever teste do caso de uso `CancelarPagamento` com o `Id` de um `Pagamento` não pendente, verificando que a exceção de domínio de cancelamento é propagada e o `Status` original é mantido
- [x] 5.4 Implementar a exceção de aplicação para "pagamento não encontrado" (distinta das exceções de domínio)
- [x] 5.5 Implementar `CancelarPagamento` orquestrando `IPagamentoRepository` e `Pagamento.Cancelar` até os testes passarem

## 6. Application — Caso de uso ConsultarPagamentos (TDD)

- [x] 6.1 Escrever teste do caso de uso `ConsultarPagamentos` com pagamentos persistidos, verificando que todos são retornados com `Id`, valor monetário e `Status`
- [x] 6.2 Escrever teste do caso de uso `ConsultarPagamentos` sem nenhum pagamento persistido, verificando que uma lista vazia é retornada
- [x] 6.3 Implementar `ConsultarPagamentos` orquestrando `IPagamentoRepository` até os testes passarem

## 7. Fechamento

- [x] 7.1 Rodar toda a suíte de testes (`Domain.Tests`, `Application.Tests`) via container/`dotnet test`, confirmando que nada quebrou
- [x] 7.2 Rodar code-review nas mudanças pendentes antes de qualquer commit
- [x] 7.3 Abrir o PR conforme o fluxo issue-driven (sincronização das specs principais fica para o `/opsx:archive`, após o merge, seguindo o padrão já usado nas mudanças anteriores deste repositório) — PR #8
