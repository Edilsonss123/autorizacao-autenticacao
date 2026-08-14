## Purpose

Garante que o Keycloak, ao subir localmente ou em CI, já disponibiliza o Realm, os Clients, as Client Roles, os Client Scopes e o mapeamento de audience necessários para que `portal-ui`, `backoffice-ui` e `parceiro-job` obtenham tokens válidos para `pagamentos-api`.

## ADDED Requirements

### Requirement: Realm do projeto disponível automaticamente
O Keycloak SHALL disponibilizar o Realm `pagamentos` automaticamente ao subir o container, sem exigir passos manuais no Admin Console.

#### Scenario: Subida do ambiente local
- **WHEN** `docker compose -f containers/docker-compose.yml up -d` é executado
- **THEN** o Realm `pagamentos` existe no Keycloak, importado a partir do `realm-export.json` versionado

### Requirement: Client pagamentos-api expõe as Client Roles do domínio
O Realm SHALL conter o Client `pagamentos-api` (Resource Server) com exatamente as Client Roles `pagamento:read`, `pagamento:create` e `pagamento:cancel`.

#### Scenario: Roles disponíveis no Client da API
- **WHEN** o Realm `pagamentos` é consultado
- **THEN** o Client `pagamentos-api` possui as roles `pagamento:read`, `pagamento:create` e `pagamento:cancel`, e nenhuma outra role de negócio

### Requirement: Clients de origem autenticam usuários via Authorization Code + PKCE
Os Clients `portal-ui` e `backoffice-ui` SHALL estar configurados como públicos (`Client authentication: OFF`), com `Standard Flow` habilitado e PKCE exigido.

#### Scenario: Portal obtém token via Authorization Code + PKCE
- **WHEN** `portal-ui` conduz um usuário pelo fluxo de login
- **THEN** o Keycloak emite um `access_token` com `azp = portal-ui`

#### Scenario: Backoffice obtém token via Authorization Code + PKCE
- **WHEN** `backoffice-ui` conduz um usuário pelo fluxo de login
- **THEN** o Keycloak emite um `access_token` com `azp = backoffice-ui`

### Requirement: Client parceiro-job autentica via Client Credentials
O Client `parceiro-job` SHALL estar configurado com `Client authentication: ON` e `Service accounts roles: ON`, autenticando sem usuário humano.

#### Scenario: Parceiro obtém token via Client Credentials
- **WHEN** `parceiro-job` solicita um token informando `client_id` e `client_secret`
- **THEN** o Keycloak emite um `access_token` com `azp = parceiro-job`, representando o Service Account do Client

### Requirement: Permissões efetivas variam por Client de origem
Cada Client de origem SHALL receber, via Client Scope dedicado com `Full Scope Allowed = OFF`, apenas o subconjunto de Client Roles de `pagamentos-api` definido para aquela origem.

#### Scenario: Portal não recebe permissão de cancelamento
- **WHEN** um usuário com a role `pagamento:cancel` atribuída no Realm autentica através de `portal-ui`
- **THEN** o `access_token` emitido não contém `pagamento:cancel` em `resource_access.pagamentos-api.roles`

#### Scenario: Backoffice recebe todas as permissões de pagamento
- **WHEN** o mesmo usuário autentica através de `backoffice-ui`
- **THEN** o `access_token` emitido contém `pagamento:read`, `pagamento:create` e `pagamento:cancel` em `resource_access.pagamentos-api.roles`

#### Scenario: Parceiro recebe apenas permissão de criação
- **WHEN** `parceiro-job` obtém um token via Client Credentials
- **THEN** o `access_token` emitido contém apenas `pagamento:create` em `resource_access.pagamentos-api.roles`

### Requirement: Tokens emitidos para a API contêm a audience correta
Os tokens emitidos para `portal-ui`, `backoffice-ui` e `parceiro-job` SHALL conter o claim `aud` incluindo `pagamentos-api`.

#### Scenario: Audience presente no token
- **WHEN** qualquer um dos três Clients (`portal-ui`, `backoffice-ui`, `parceiro-job`) obtém um `access_token`
- **THEN** o claim `aud` do token inclui `pagamentos-api`

### Requirement: Usuário e grupo de teste demonstram permissão dependente do Client
O Realm SHALL conter ao menos um usuário de teste com acesso a `portal-ui` e a `backoffice-ui`, e um grupo `operadores-backoffice` com as roles de Backoffice atribuídas.

#### Scenario: Mesmo usuário, permissões diferentes por origem
- **WHEN** o usuário de teste autentica primeiro por `portal-ui` e depois por `backoffice-ui`
- **THEN** o `sub` permanece o mesmo nos dois tokens, mas o conjunto de roles em `resource_access.pagamentos-api.roles` muda conforme o Client usado
