# Roadmap

Lista de atividades necessárias para o projeto ficar pronto, do zero até uma API de pagamentos funcional com autenticação/autorização via Keycloak.

Este documento é um guia de planejamento, não um change do OpenSpec. Cada parte (ou grupo de itens dentro dela) deve virar um change via `/opsx:propose` antes de ser implementada — nada é codificado sem proposta e spec revisadas (ver `README.md` e `openspec/config.yaml`). Sempre seguir `AGENTS.md` e `docs/arquitetura.md`.

Convenção: 

- [ ] pendente
- [x] concluído 

---

## Parte 1 — Subir o Keycloak

- [x] `containers/docker-compose.yml` com o serviço `keycloak` (imagem `quay.io/keycloak/keycloak:26.7.1`, `start-dev --import-realm`, volume em `containers/data/keycloak`)
- [x] Subir com `docker compose -f containers/docker-compose.yml up -d` e validar acesso ao Admin Console (`http://localhost:7050`)
- [x] Decidir e documentar as credenciais de admin do ambiente local (nunca reaproveitar em produção) — `admin`/`admin`, valores de laboratório, documentado em `docs/arquitetura.md`



## Parte 2 — Realm

- [x] Criar o Realm do projeto — nome definido: `pagamentos` (versionado em `containers/config/keycloak/realm-export.json`)
- [x] Documentar a decisão: por que um único Realm é suficiente para as origens deste projeto (ver `openspec/changes/configuracao-keycloak/design.md` e seção 57/58.10 do `README.md`)



## Parte 3 — Client da API (Resource Server)

- [x] Criar Client `pagamentos-api` (OpenID Connect, representa o Resource Server)
- [x] Criar as Client Roles em `pagamentos-api`: `pagamento:read`, `pagamento:create`, `pagamento:cancel`



## Parte 4 — Clients de origem

- [x] Criar `portal-ui` (SPA — `Client authentication: OFF`, `Standard flow: ON`, Authorization Code + PKCE)
- [x] Criar `backoffice-ui` (mesmo padrão do `portal-ui`, redirect URIs próprias)
- [x] Criar `parceiro-job` (`Client authentication: ON`, `Service accounts roles: ON`, Client Credentials)
- [x] Configurar Valid Redirect URIs / Web Origins de laboratório para cada Client (`5173`/`5174`) — revisar quando o frontend real existir



## Parte 5 — Client Scopes, Role Scope Mappings e Audience

- [x] Criar Client Scopes por contexto de acesso (`pagamentos-portal`, `pagamentos-backoffice`)
- [x] Configurar Role Scope Mappings de cada scope com o subconjunto de roles permitido
- [x] Associar cada scope ao Client de origem correspondente e desabilitar `Full Scope Allowed`
- [x] Configurar Audience Mapper para que os tokens emitidos contenham `aud = pagamentos-api`
- [x] Configurar Role Mapping do Service Account de `parceiro-job` (`pagamento:create`)



## Parte 6 — Usuários e grupos de teste

- [x] Criar usuários de teste (`joao`) com acesso a `portal-ui` e `backoffice-ui`
- [x] Criar grupos (`operadores-backoffice`) e associar roles ao grupo em vez de usuário a usuário
- [x] Validar que o mesmo usuário recebe permissões diferentes dependendo do Client usado (seção 17 do `README.md`) — confirmado: mesmo `sub`, roles diferentes por `azp`



## Parte 7 — Estrutura da solução .NET

- [x] Aplicar o change `openspec/changes/scaffold-estrutura-solucao` (solution `.slnx`, projetos `Domain`/`Application`/`Infrastructure`/`Api` + respectivos `.Tests`, referências na direção correta) — já proposto, falta aplicar via `/opsx:apply`



## Parte 8 — Domain (DDD + TDD)

- [x] Modelar a entidade `Pagamento` e Value Objects necessários (valor monetário, status, etc.), sem depender de nenhuma outra camada
- [x] Implementar regras de negócio: criação, cancelamento (ex.: só cancela se `Status == Pendente`), invariantes do agregado
- [x] Escrever os testes de domínio antes da implementação (`AutorizacaoAutenticacao.Domain.Tests`)



## Parte 9 — Application (casos de uso e ports)

- [x] Definir os ports (`IPagamentoRepository`, `ICallerContext`, etc.) em `Application`
- [x] Implementar os casos de uso: `ConsultarPagamentos`, `CriarPagamento`, `CancelarPagamento`
- [x] Testes de aplicação com fakes/in-memory para os ports (sem infraestrutura real)



## Parte 10 — Infrastructure (adapters)

- [ ] Escolher e configurar persistência (EF Core + provider) e implementar `PagamentoRepository`
- [ ] Implementar `KeycloakRolesClaimsTransformation` (normaliza `resource_access` → claim `permission`)
- [ ] Implementar `CallerContext` (adapter de `ICallerContext` sobre `HttpContext`/claims `sub`/`azp`)
- [ ] Testes de integração rodando contra os containers definidos em `containers/`



## Parte 11 — Api (Minimal API)

- [ ] Configurar `AddAuthentication` + `AddJwtBearer` (Authority/Audience via `appsettings`, `MapInboundClaims = false`)
- [ ] Configurar `AddAuthorization` com Policies como constantes (`Policies.PagamentoRead`, `Policies.PagamentoCreate`, `Policies.PagamentoCancel`)
- [ ] Endpoints Minimal API agrupados por capacidade (`MapGroup`): `GET/POST /api/pagamentos`, `DELETE /api/pagamentos/{id}`, cada um com `RequireAuthorization` na policy correta
- [ ] Endpoint de diagnóstico `GET /api/me` (retorna `sub`/`azp` do token) — só para ambientes de desenvolvimento
- [ ] Validators de request com FluentValidation, próximos aos endpoints



## Parte 12 — Autorização e segurança adicionais

- [ ] `AuthorizationHandler` para regras contextuais (ex.: cancelar só se `pagamento.EmpresaId == usuario.EmpresaId`)
- [ ] Policy combinada `azp` + `permission` apenas onde o negócio exigir explicitamente a identidade do Client (ex.: cancelamento restrito ao Backoffice)
- [ ] Constantes `Permissions`/`Policies` centralizadas (nunca strings soltas)
- [ ] Auditoria de operações sensíveis (log estruturado com `sub`, `azp`, ação, recurso, resultado — nunca o JWT completo, `refresh_token` ou `client_secret`)



## Parte 13 — Testes de autorização (matriz)

Cobrir, no mínimo, os cenários da seção 45 do `README.md`:

- [ ] Sem token → `401`
- [ ] Portal com `pagamento:read` → `200` em `GET /api/pagamentos`
- [ ] Portal tentando `DELETE` (sem `pagamento:cancel`) → `403`
- [ ] Backoffice cancelando com `pagamento:cancel` → `200`/`204`
- [ ] Parceiro criando com `pagamento:create` → `200`/`201`
- [ ] Parceiro tentando cancelar (sem a permissão) → `403`
- [ ] Token com `aud` incorreta → `401`
- [ ] Token expirado → `401`
- [ ] Header `X-Origin` falsificado não altera a origem real (`azp` continua vindo do token) → autorização ignora o header



## Parte 14 — Finalização

- [ ] Revisar checklist de segurança: HTTPS em produção, Authorization Code + PKCE para públicos, Client Credentials para M2M, access tokens de curta duração, nenhum segredo em SPA
- [ ] `appsettings` por ambiente (Authority/Audience de dev/homologação/produção)
- [ ] Atualizar `README.md` com instruções de setup do projeto real (distinguindo do material de treinamento já presente)
- [ ] `dotnet build` e `dotnet test` da solution completa sem erros, cobrindo os cenários da Parte 13