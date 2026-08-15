# AGENTS.md

Regras que todo agente de IA (Claude, Cursor, Codex, OpenSpec, etc.) deve seguir ao trabalhar neste repositório.

## Idioma

Tudo neste repositório é escrito em português: comunicação, documentação, comentários de commit **e código** (nomes de classes, métodos, variáveis, propriedades, etc.).

Exceção: termos e padrões amplamente adotados na indústria/na linguagem permanecem em inglês — por exemplo `Repository`, `Handler`, `Service`, `Controller`, `DTO`, `Factory`, `Builder`, palavras-chave da linguagem (`async`/`await`, `class`, `interface`), tipos do framework (`Guid`, `Task`, `CancellationToken`) e claims/termos de protocolo (`sub`, `azp`, `aud`, `Bearer`). Na dúvida, prefira português.

Exceção adicional: arquivos gerados/vendorizados por ferramentas de terceiros (ex.: os mirrors de skills/commands do `openspec` CLI em `.agents/`, `.claude/skills/openspec-*`, `.claude/commands/opsx/`, `.cursor/`) permanecem no idioma original gerado pela ferramenta — traduzi-los manualmente quebraria a regeneração automática. Isso não se aplica a nada escrito por um agente ou humano deste projeto.

Exemplo:
```csharp
public sealed class CancelarPagamentoCasoDeUso
{
    public async Task<Pagamento> ExecutarAsync(Guid pagamentoId, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

## Visão geral do projeto

API .NET de pagamentos com autenticação/autorização via Keycloak (OIDC). Ver `README.md` para o modelo de segurança (claims `sub`/`azp`/`aud`, policies, 401 vs 403).

Stack: .NET / ASP.NET Core, Keycloak, Docker.

## Arquitetura

A arquitetura do projeto (Hexagonal/Ports & Adapters + DDD, estrutura de pastas, regras de API) está descrita em [`docs/arquitetura.md`](docs/arquitetura.md). Leia e siga esse documento antes de criar ou alterar código.

## TDD

- Todo código de Domain e Application deve ser desenvolvido com teste primeiro: escreva o teste que falha, depois o código mínimo para passá-lo, depois refatore.
- Testes de Domain/Application não devem depender de infraestrutura real (sem banco, sem Keycloak, sem HTTP) — use fakes/in-memory nos ports.
- Testes de Infrastructure (adapters) podem ser testes de integração, rodando contra os containers definidos em `containers/`.
- Nenhum código de produção é aceito sem teste correspondente cobrindo o caso de uso.

## Autenticação e autorização

Seguir o contrato documentado no `README.md`:
- Autorizar por `permission` (claims normalizadas a partir de `resource_access`), não por nome de origem (`azp`), exceto quando o requisito de negócio exigir explicitamente a identidade do client.
- Validar sempre `issuer`, `audience`, `signature` e `expiração`.
- Nunca usar `Origin`, `Referer` ou headers customizados como mecanismo de autorização.
- Distinguir corretamente `401` (token ausente/inválido) de `403` (token válido sem permissão).
- Nunca logar o JWT completo.

## Containers

- Toda a infraestrutura de containers (docker-compose, Dockerfiles) fica em `containers/`.
- Volumes de dados ficam em `containers/data/` — essa pasta é ignorada pelo git (exceto `.gitkeep`); nunca versionar dados de volumes.
- Subir o ambiente local com `docker compose -f containers/docker-compose.yml up -d`.

## Convenções de código C#/.NET

- Nullable reference types habilitado; evitar `!` de supressão salvo necessidade comprovada.
- Permissões e nomes de policies como constantes (`Permissions`, `Policies`), nunca strings soltas espalhadas pelo código.
- Preferir `sealed class` para implementações que não são projetadas para herança.

## OpenSpec

Todo change proposto ou aplicado via OpenSpec deve respeitar as regras deste arquivo e de `docs/arquitetura.md` (arquitetura hexagonal, TDD, contrato de segurança, localização de containers). Isso está referenciado em `openspec/config.yaml`.
