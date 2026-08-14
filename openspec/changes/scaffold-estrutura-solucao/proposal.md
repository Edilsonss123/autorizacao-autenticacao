## Why

O repositório hoje só tem documentação e infraestrutura de containers — nenhum código .NET existe. Antes de implementar qualquer caso de uso, é preciso ter a solution e o esqueleto de projetos no lugar, já respeitando a Arquitetura Hexagonal e a regra de que o Domain não pode depender de nenhuma outra camada.

## What Changes

- Criar a pasta `src/` na raiz do repositório.
- Criar a solution `AutorizacaoAutenticacao.slnx` (formato `.slnx`, novo formato de solution do SDK .NET) dentro de `src/`.
- Criar os projetos de código, vazios (sem casos de uso reais ainda):
  - `AutorizacaoAutenticacao.Domain`
  - `AutorizacaoAutenticacao.Application`
  - `AutorizacaoAutenticacao.Infrastructure`
  - `AutorizacaoAutenticacao.Api`
- Criar os projetos de teste, vazios:
  - `AutorizacaoAutenticacao.Domain.Tests`
  - `AutorizacaoAutenticacao.Application.Tests`
  - `AutorizacaoAutenticacao.Infrastructure.Tests` (testes de integração, cobrindo também `Api`)
- Configurar as referências entre projetos de forma que a direção de dependência da Arquitetura Hexagonal seja garantida estruturalmente desde o primeiro commit.
- Adicionar todos os projetos à solution.

Nenhum caso de uso, entidade, endpoint ou regra de negócio é implementado nesta change — apenas a estrutura vazia.

## Capabilities

Esta change é scaffolding puro (estrutura de projeto/tooling), sem comportamento de sistema observável. `skip_specs: true` está definido em `.openspec.yaml`; nenhuma capability nova ou modificada é declarada.

## Impact

- **Novo diretório**: `src/` e todos os seus subprojetos.
- **Novo arquivo**: `src/AutorizacaoAutenticacao.slnx`.
- Não afeta `README.md`, `AGENTS.md`, `docs/arquitetura.md` ou `containers/` — apenas os consome como referência.
- Nenhuma dependência externa (NuGet) é adicionada nesta change, além do necessário para os projetos existirem e compilarem (SDK padrão do .NET, framework de testes).
