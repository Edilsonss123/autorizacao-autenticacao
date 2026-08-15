## Context

`AutorizacaoAutenticacao.Domain` e `AutorizacaoAutenticacao.Domain.Tests` já existem como projetos vazios (Parte 7 do roadmap, aplicada). Não há nenhum tipo de domínio ainda. Ver `proposal.md` para a motivação.

## Goals / Non-Goals

**Goals:**
- Definir a forma dos tipos de domínio (`Pagamento`, Value Objects, exceções) o suficiente para orientar os testes e a implementação.
- Garantir que nenhuma invariante do agregado possa ser violada através da API pública dos tipos (sem setters públicos, sem estado inválido representável).

**Non-Goals:**
- Persistência, mapeamento ORM ou qualquer preocupação de Infrastructure.
- Casos de uso, ports ou orquestração (Application) — isso é a Parte 9 do roadmap, change futura.
- Suporte a múltiplas moedas — o valor monetário assume uma única moeda implícita do sistema (sem campo de moeda) nesta change; se/quando o negócio exigir multi-moeda, isso será uma change própria.

## Decisions

### Nomes em português, seguindo AGENTS.md
`Pagamento`, `ValorMonetario`, `StatusPagamento`. Termos de framework/protocolo permanecem em inglês (ex.: `Guid` para o identificador). Alternativa descartada: nomes em inglês (`Payment`, `Money`) — rejeitada porque contraria a regra do projeto.

### `StatusPagamento` como enum, não Value Object com comportamento
O status é um conjunto fechado e pequeno de estados (`Pendente`, `Cancelado`). Um `enum` é suficiente e mais simples que uma classe de Value Object dedicada. A regra de transição ("só cancela se `Pendente`") fica no método `Cancelar()` do agregado `Pagamento`, não no enum — o enum não tem comportamento, é só o dado.

### `ValorMonetario` como Value Object imutável (`sealed class`, não `record`)
Precisa impor a invariante "> 0" na própria construção (não é possível existir um `ValorMonetario` inválido) e oferecer igualdade por valor. Implementação: `sealed class` com construtor **privado**, método estático `Criar(decimal montante)` como único ponto de entrada (valida e lança exceção de domínio se `<= 0`), propriedade `Montante` `get`-only, e `Equals`/`GetHashCode`/`operator ==`/`operator !=` sobrescritos manualmente para igualdade por valor.

Alternativas descartadas:
- `decimal` cru como parâmetro do `Pagamento` — rejeitada porque permite valores inválidos circularem pelo domínio sem validação centralizada.
- `record`/`readonly record struct` posicional (considerada inicialmente por dar igualdade por valor "de graça") — rejeitada porque a expressão `with` do C# usa um construtor de cópia que **não** reexecuta a validação do construtor primário, permitindo furar a invariante (`valor with { Montante = -10 }` criaria uma instância inválida sem lançar exceção). Uma `sealed class` comum simplesmente não compila `with { ... }` fora de `record`/`record struct` — elimina o caminho de bypass estruturalmente, em vez de mitigá-lo em runtime.

### Exceção de domínio dedicada
Regras violadas (valor inválido na criação, cancelamento de pagamento não pendente) lançam uma exceção de domínio específica (ex.: `DomainException` base ou exceções específicas por caso), nunca `ArgumentException`/`InvalidOperationException` genéricas do BCL, para deixar explícito que a falha é uma regra de negócio e não um erro de uso da API de tipos. Detalhe de nomenclatura das classes de exceção fica para a implementação (guiado pelos testes), não é uma decisão que precise ser fixada aqui.

### `Pagamento` sem construtor público "cru"
A criação acontece por um método/factory que já aplica as invariantes (ex.: construtor que valida, ou método estático `Criar(...)`), garantindo que não existe caminho para instanciar um `Pagamento` em estado inválido. A escolha entre construtor validante e factory method estático fica para a implementação/testes — ambas satisfazem a invariante; não é uma decisão observável pela spec.

### `Pagamento.Criar(...)` recebe o montante bruto, não um `ValorMonetario` pronto
`Pagamento.Criar(decimal montante, ...)` recebe o `decimal` cru e constrói o `ValorMonetario` internamente (`ValorMonetario.Criar(montante)`), em vez de receber um `ValorMonetario` já validado. Isso torna `Pagamento.Criar` diretamente testável pelos dois cenários de criação inválida do `spec.md` que são fraseados em nível de `Pagamento` ("Tentativa de criação com valor zero/negativo"), sem depender apenas de cobertura indireta via `ValorMonetarioTests`. Os dois níveis de teste (`PagamentoTests` e `ValorMonetarioTests`) acabam exercitando o mesmo código de validação por baixo, mas cada um ancora um requirement distinto do spec — não é duplicação.

## Risks / Trade-offs

- [Risco] Modelar `StatusPagamento` como enum simples pode não escalar se novas transições complexas (com metadados por transição) surgirem depois → Mitigação: aceitável agora porque a spec desta change só exige duas transições triviais; revisitar via nova change se o negócio pedir mais estados (ex.: `Aprovado`, `Estornado`).
- [Risco] Sem campo de moeda no `ValorMonetario`, uma necessidade futura de multi-moeda exigirá migração do Value Object → Mitigação: escopo explícito nas Non-Goals; decisão consciente, não descuido.
