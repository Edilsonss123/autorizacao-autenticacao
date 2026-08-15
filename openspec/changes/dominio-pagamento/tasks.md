## 1. Preparação (branch)

- [x] 1.1 Verificar se a branch atual corresponde a esta alteração (`git status`/`git branch`) e não é a `main` nem a branch de outra tarefa
- [x] 1.2 Se não estiver na branch correta, criar a branch (padrão `type/description` da skill `issue-driven-github-flow`) e publicar a branch remota correspondente antes de iniciar qualquer edição

## 2. ValorMonetario (Value Object)

- [x] 2.1 Escrever teste: criar `ValorMonetario` com montante positivo tem sucesso
- [x] 2.2 Escrever teste: criar `ValorMonetario` com montante zero lança exceção de domínio
- [x] 2.3 Escrever teste: criar `ValorMonetario` com montante negativo lança exceção de domínio
- [x] 2.4 Escrever teste: dois `ValorMonetario` com o mesmo montante são iguais (incluindo `Equals(null)` e comparação via operador `==`/`!=` com um dos lados `null`)
- [x] 2.5 Implementar `ValorMonetario` como `sealed class` (construtor privado, `Criar(decimal)` estático, `Equals`/`GetHashCode`/`operator ==`/`operator !=` manuais — não usar `record`, ver `design.md`) até os testes acima passarem

## 3. StatusPagamento e exceções de domínio

- [x] 3.1 Implementar enum `StatusPagamento` (`Pendente`, `Cancelado`)
- [x] 3.2 Implementar a(s) exceção(ões) de domínio usada(s) pelas invariantes do agregado (ver `design.md` — Decisions)

## 4. Pagamento — criação

- [x] 4.1 Escrever teste: criar `Pagamento` com montante `decimal` válido resulta em `Status = Pendente`
- [x] 4.2 Escrever teste: `Pagamento.Criar` com montante zero lança exceção de domínio (`Criar_ComValorZero_LancaExcecaoDeDominio`)
- [x] 4.3 Escrever teste: `Pagamento.Criar` com montante negativo lança exceção de domínio (`Criar_ComValorNegativo_LancaExcecaoDeDominio`)
- [x] 4.4 Implementar `Pagamento.Criar(decimal montante, ...)` (constrói `ValorMonetario` internamente via `ValorMonetario.Criar`) até os testes acima passarem

## 5. Pagamento — cancelamento

- [x] 5.1 Escrever teste: cancelar `Pagamento` com `Status = Pendente` resulta em `Status = Cancelado`
- [x] 5.2 Escrever teste: cancelar `Pagamento` com `Status = Cancelado` lança exceção de domínio e mantém `Status = Cancelado`
- [x] 5.3 Implementar `Cancelar()` no agregado `Pagamento` até os testes acima passarem

## 6. Fechamento

- [x] 6.1 Rodar `dotnet test` do projeto `AutorizacaoAutenticacao.Domain.Tests` e confirmar que todos os testes passam
- [x] 6.2 Revisar que `AutorizacaoAutenticacao.Domain` não ganhou nenhuma referência a `Application`/`Infrastructure`/`Api`
- [x] 6.3 Marcar os itens da Parte 8 como concluídos em `docs/roadmap.md`
