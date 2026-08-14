## 1. Preparação

- [x] 1.1 Subir o Keycloak atual (`docker compose -f containers/docker-compose.yml up -d`) e acessar o Admin Console para montar a configuração antes de exportar
- [x] 1.2 Criar o Realm `pagamentos` no Admin Console

## 2. Client pagamentos-api (Resource Server)

- [x] 2.1 Criar o Client `pagamentos-api` (OpenID Connect, `Client authentication: ON`, sem Standard Flow — usado apenas como recurso)
- [x] 2.2 Criar as Client Roles `pagamento:read`, `pagamento:create`, `pagamento:cancel` em `pagamentos-api`

## 3. Clients de origem (usuário)

- [x] 3.1 Criar `portal-ui` (`Client authentication: OFF`, `Standard flow: ON`, PKCE)
- [x] 3.2 Criar `backoffice-ui` (mesmo padrão de `portal-ui`)
- [x] 3.3 Definir Valid Redirect URIs / Web Origins de laboratório para ambos (ex.: `http://localhost:5173/*` e `http://localhost:5174/*`)

## 4. Client parceiro-job (machine-to-machine)

- [x] 4.1 Criar `parceiro-job` (`Client authentication: ON`, `Standard flow: OFF`, `Service accounts roles: ON`)
- [x] 4.2 Em Service Account Roles de `parceiro-job`, atribuir a Client Role `pagamento:create` de `pagamentos-api`

## 5. Client Scopes e Role Scope Mappings

- [x] 5.1 Criar o Client Scope `pagamentos-portal` com Role Scope Mapping para `pagamento:read` e `pagamento:create` de `pagamentos-api`
- [x] 5.2 Criar o Client Scope `pagamentos-backoffice` com Role Scope Mapping para `pagamento:read`, `pagamento:create` e `pagamento:cancel`
- [x] 5.3 Associar `pagamentos-portal` ao Client `portal-ui` e `pagamentos-backoffice` ao Client `backoffice-ui`
- [x] 5.4 Desabilitar `Full Scope Allowed` em `portal-ui` e `backoffice-ui`

## 6. Audience

- [x] 6.1 Adicionar um Audience Mapper (em `pagamentos-portal` e `pagamentos-backoffice`, ou em um Client Scope compartilhado) com `Included Client Audience = pagamentos-api`
- [x] 6.2 Garantir que o token do Service Account de `parceiro-job` também contenha `aud = pagamentos-api` (mapper direto no Client, já que ele não usa os Client Scopes de origem)

## 7. Usuários e grupo de teste

- [x] 7.1 Criar um usuário de teste (ex.: `joao`) com senha de laboratório
- [x] 7.2 Criar o grupo `operadores-backoffice` com a role `pagamento:cancel` (além de `read`/`create`) atribuída ao grupo
- [x] 7.3 Adicionar o usuário de teste ao grupo `operadores-backoffice`

## 8. Exportação e automação

- [x] 8.1 Exportar o Realm configurado (via Admin Console ou `kc.sh export`) para `containers/config/keycloak/realm-export.json`
- [x] 8.2 Atualizar `containers/docker-compose.yml`: montar `realm-export.json` em `/opt/keycloak/data/import/` e ajustar `command` para `start-dev --import-realm`
- [x] 8.3 Adicionar ao `.gitignore` apenas os dados de runtime do Keycloak (`containers/data/keycloak`), garantindo que `realm-export.json` fique versionado

## 9. Validação

- [x] 9.1 Recriar o ambiente do zero (`docker compose down -v` seguido de `docker compose up -d`) e confirmar que o Realm `pagamentos` já existe sem passos manuais
- [x] 9.2 Obter um token de `portal-ui` (Authorization Code + PKCE, ex. via Postman) e confirmar `azp = portal-ui`, `aud` contendo `pagamentos-api` e roles restritas a `pagamento:read`/`pagamento:create`
- [x] 9.3 Obter um token de `backoffice-ui` com o mesmo usuário e confirmar as três roles (`read`/`create`/`cancel`) presentes
- [x] 9.4 Obter um token de `parceiro-job` via Client Credentials e confirmar `azp = parceiro-job`, `aud` contendo `pagamentos-api` e apenas `pagamento:create`
- [x] 9.5 Documentar em `docs/arquitetura.md` como reexportar `realm-export.json` após qualquer alteração feita no Admin Console
