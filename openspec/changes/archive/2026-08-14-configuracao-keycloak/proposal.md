## Why

O Keycloak já sobe via `containers/docker-compose.yml`, mas nenhum Realm, Client, Role ou Client Scope existe ainda. Sem essa configuração nenhuma aplicação (`portal-ui`, `backoffice-ui`, `parceiro-job`) consegue obter um token válido para `pagamentos-api`, o que bloqueia toda a implementação de autenticação/autorização da API (Minimal API, Policies) e os testes de integração da matriz de autorização.

## What Changes

- Criar um `realm-export.json` versionado em `containers/` contendo:
  - O Realm do projeto (`pagamentos`)
  - O Client `pagamentos-api` (Resource Server) com as Client Roles `pagamento:read`, `pagamento:create`, `pagamento:cancel`
  - Os Clients de origem `portal-ui` e `backoffice-ui` (Authorization Code + PKCE, `Client authentication: OFF`)
  - O Client `parceiro-job` (Client Credentials, `Service accounts roles: ON`) com o Service Account já mapeado para `pagamento:create`
  - Os Client Scopes `pagamentos-portal` (`pagamento:read`, `pagamento:create`) e `pagamentos-backoffice` (`pagamento:read`, `pagamento:create`, `pagamento:cancel`), associados aos respectivos Clients com `Full Scope Allowed = OFF`
  - Um Audience Mapper garantindo `aud = pagamentos-api` nos tokens emitidos para `portal-ui` e `backoffice-ui`
  - Usuários e grupo de teste (`operadores-backoffice`) para validar que o mesmo usuário recebe permissões diferentes conforme o Client usado
- Atualizar `containers/docker-compose.yml` para importar esse realm automaticamente na subida do container (`--import-realm`), montando o arquivo como volume
- Documentar em `docs/arquitetura.md` (ou `README.md`) como reimportar/regenerar o `realm-export.json` quando a configuração mudar

**BREAKING**: nenhuma — não existe configuração anterior de Realm a ser substituída.

## Capabilities

### New Capabilities

- `keycloak-realm`: configuração do Realm, Clients, Client Roles, Client Scopes e Audience do Keycloak que sustentam o modelo de autenticação/autorização de `pagamentos-api` descrito no `README.md`.

### Modified Capabilities

(nenhuma — não existe spec anterior de identidade)

## Impact

- **Novo arquivo**: `containers/config/keycloak/realm-export.json`
- **Modificado**: `containers/docker-compose.yml` (import automático do realm)
- Não afeta código .NET (`src/`) diretamente nesta change — mas é pré-requisito para a Parte 11 do roadmap (`Api`: `AddAuthentication`/`AddJwtBearer` apontando para este Realm) e para a Parte 13 (testes de integração da matriz de autorização)
- Credenciais/segredos de Client (`parceiro-job`, `client_secret`) gerados neste change são valores de laboratório/dev — nunca reaproveitáveis em produção
