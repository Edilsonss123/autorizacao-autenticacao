## Why

A solution já tem o esqueleto de projetos (`AutorizacaoAutenticacao.Domain` e `AutorizacaoAutenticacao.Domain.Tests` existem, mas vazios). Nenhuma regra de negócio de pagamento está modelada ainda. Antes de existir qualquer caso de uso, endpoint ou persistência, o núcleo do domínio — a entidade `Pagamento`, seus Value Objects e as invariantes que protegem o agregado — precisa existir de forma isolada, sem depender de nenhuma outra camada, e nascer com os testes que a especificam (TDD).

## What Changes

- Modelar a entidade `Pagamento` (aggregate root) em `AutorizacaoAutenticacao.Domain`, sem depender de `Application`, `Infrastructure` ou `Api`.
- Modelar os Value Objects necessários:
  - Valor monetário (imutável, não aceita valores negativos ou zero).
  - `StatusPagamento` (ex.: `Pendente`, `Cancelado` — o conjunto mínimo exigido pelas regras desta change).
- Implementar as regras de negócio do agregado:
  - Criação de um `Pagamento` (estado inicial: `Pendente`).
  - Cancelamento de um `Pagamento`, permitido apenas quando `Status == Pendente`; tentativa de cancelar um pagamento que não está `Pendente` deve falhar de forma explícita (exceção de domínio), nunca silenciosamente.
  - Invariantes do agregado garantidas no próprio construtor/métodos (não é possível construir um `Pagamento` em estado inválido).
- Escrever os testes de domínio em `AutorizacaoAutenticacao.Domain.Tests` **antes** da implementação (ciclo red-green-refactor), cobrindo criação válida, criação inválida, cancelamento válido e cancelamento inválido.

Nenhum port, caso de uso, endpoint ou persistência é criado nesta change — apenas o núcleo do domínio.

## Capabilities

### New Capabilities
- `pagamento-dominio`: regras de negócio do agregado `Pagamento` — criação, cancelamento e invariantes, independentes de qualquer camada externa.

### Modified Capabilities
(nenhuma)

## Impact

- **Novo código**: entidade `Pagamento`, Value Objects (valor monetário, `StatusPagamento`) e exceções de domínio em `src/AutorizacaoAutenticacao.Domain`.
- **Novo código de teste**: testes unitários em `src/AutorizacaoAutenticacao.Domain.Tests`.
- Não afeta `Application`, `Infrastructure`, `Api` ou os containers — é pré-requisito para a Parte 9 do roadmap (`openspec/changes/../docs/roadmap.md`, casos de uso e ports).
- Nenhuma dependência externa (NuGet) é adicionada além do framework de testes já referenciado no scaffold.
