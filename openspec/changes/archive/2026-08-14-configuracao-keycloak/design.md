## Context

Ver `proposal.md` - Why. O Keycloak (`containers/docker-compose.yml`) já sobe com `start-dev`, mas sem Realm/Clients/Roles. O material de treinamento em `README.md` (seções 9-22) descreve a mesma configuração através de cliques manuais no Admin Console. Esta change substitui os cliques manuais por um artefato declarativo versionado, importado automaticamente na subida do container.

## Goals / Non-Goals

**Goals:**
- Realm, Clients, Client Roles, Client Scopes e Audience Mapper reproduzíveis a partir de um único arquivo versionado.
- Ambiente idêntico em qualquer máquina/CI ao rodar `docker compose up -d`, sem passos manuais.
- Habilitar a Parte 13 do roadmap (matriz de testes de autorização) a rodar contra um Keycloak com estado conhecido.

**Non-Goals:**
- Configurar a API .NET (`AddAuthentication`/`AddJwtBearer`, Policies) — é a Parte 11 do roadmap, change separada.
- Definir estratégia de produção para gestão de segredos do Keycloak (vault, rotação de `client_secret`) — fora do escopo deste ambiente de desenvolvimento/laboratório.
- Automatizar a geração do `realm-export.json` via pipeline — o arquivo é escrito/mantido manualmente nesta change.

## Decisions

### Realm export versionado, importado via `--import-realm`
Optamos por manter um `realm-export.json` em `containers/config/keycloak/realm-export.json` e configurar o serviço `keycloak` no `docker-compose.yml` para importá-lo automaticamente (`command: start-dev --import-realm`, montando o arquivo em `/opt/keycloak/data/import/`).

Alternativas consideradas:
- **Cliques manuais no Admin Console** (estilo do tutorial no `README.md`): rejeitado como formato de entrega desta change porque não é reproduzível nem testável automaticamente — cada ambiente exigiria repetir os passos, e a Parte 13 (testes de integração) não teria um estado inicial garantido.
- **Script `kcadm.sh`**: reproduzível, mas mais verboso de escrever/manter do que um único JSON declarativo; o export cobre naturalmente Realm, Clients, Roles, Scopes e Mappers em uma estrutura já suportada nativamente pelo Keycloak.

### Nome do Realm: `pagamentos`
Um único Realm compartilhado por todas as origens (`portal-ui`, `backoffice-ui`, `parceiro-job`, `pagamentos-api`), em vez de um Realm por aplicação — consistente com a seção 57/58.10 do `README.md`: não há necessidade de isolamento forte (usuários, políticas de senha, MFA ou IdPs distintos) entre essas origens.

### Client Scopes dedicados por contexto de acesso
Cada Client de origem recebe um Client Scope próprio (`pagamentos-portal`, `pagamentos-backoffice`) com Role Scope Mapping restrito e `Full Scope Allowed = OFF`, em vez de atribuir as Client Roles diretamente a cada Client. Isso deixa explícito, em um único lugar, qual subconjunto de permissões cada origem pode receber — essencial para o cenário "mesmo usuário, permissões diferentes por aplicação" (`README.md`, seção 17).

### Segredos de desenvolvimento
`client_secret` de `parceiro-job` e a senha do usuário de teste ficam com valores fixos de laboratório no `realm-export.json`, documentados como não reutilizáveis em produção (mesma prática já adotada para `KC_BOOTSTRAP_ADMIN_PASSWORD` no `docker-compose.yml` atual).

## Risks / Trade-offs

- [Risco] `realm-export.json` desdesatualizado em relação ao que foi validado manualmente no Admin Console → Mitigação: qualquer alteração de configuração deve ser refletida no arquivo versionado antes de ser considerada concluída; o Admin Console só é usado para inspeção/depuração, não como fonte de verdade.
- [Risco] Formato do export pode variar entre versões do Keycloak → Mitigação: gerar o export a partir da própria imagem fixada em `docker-compose.yml` (`quay.io/keycloak/keycloak:26.7.1`), garantindo compatibilidade.
- [Trade-off] Segredos de Client ficam em texto plano no repositório → aceitável neste estágio por serem exclusivamente valores de desenvolvimento/laboratório, nunca usados fora do ambiente local; revisado nas Partes 6 e 14 do roadmap antes de qualquer configuração de produção.
