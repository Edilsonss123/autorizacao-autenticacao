# Autorização e Autenticação

API .NET de pagamentos com autenticação/autorização via Keycloak (OIDC), construída em **DDD**, **TDD**, **Arquitetura Hexagonal (Ports & Adapters)** e executada em **containers** (Docker).

Repositório: https://github.com/Edilsonss123/autorizacao-autenticacao

## Stack

- .NET / ASP.NET Core
- Keycloak (OpenID Connect / OAuth2)
- Docker / Docker Compose
- Domain-Driven Design
- Test-Driven Development
- Arquitetura Hexagonal (Ports & Adapters)

## Desenvolvimento guiado por OpenSpec

A criação deste projeto é guiada pelo **[OpenSpec](https://github.com/Fission-AI/OpenSpec)**: nada é implementado sem antes existir uma proposta e uma spec revisadas. Guia oficial: [Getting Started](https://github.com/Fission-AI/OpenSpec/blob/main/docs/getting-started.md).

As regras específicas deste repositório (idioma, arquitetura, TDD, etc.) estão em [`AGENTS.md`](AGENTS.md) e [`docs/arquitetura.md`](docs/arquitetura.md) — o OpenSpec sempre as considera (ver `openspec/config.yaml`).

### Fluxo de criação de uma mudança

```text
/opsx:explore   (opcional — pensar na ideia antes de propor)
/opsx:propose   (a IA cria proposta + specs + design + tasks)
/opsx:apply     (a IA implementa as tasks)
/opsx:archive   (specs mescladas em openspec/specs/, change arquivada)
```

Comandos `/opsx:...` são digitados no chat com o assistente de IA. Comandos `openspec ...` são digitados no terminal.

### Principais comandos

| Comando | Onde | O que faz |
|---|---|---|
| `openspec init` | terminal | Inicializa o OpenSpec no projeto |
| `/opsx:explore` | chat | Explora uma ideia antes de propor uma mudança |
| `/opsx:propose <nome>` | chat | Cria a proposta, specs, design e tasks de uma mudança |
| `/opsx:apply` | chat | Implementa as tasks da mudança |
| `/opsx:archive` | chat | Mescla as specs e arquiva a mudança concluída |
| `openspec list` | terminal | Lista as mudanças ativas |
| `openspec show <nome>` | terminal | Mostra os detalhes de uma mudança |
| `openspec validate <nome>` | terminal | Valida a formatação das specs |
| `openspec view` | terminal | Abre o dashboard interativo |

## Documentação

O conteúdo abaixo é o material de treinamento/referência sobre autenticação e autorização com Keycloak + ASP.NET Core que fundamenta as decisões de segurança deste projeto.

---

# Treinamento — Keycloak + ASP.NET Core para múltiplas origens

> Objetivo: autenticar diferentes aplicações em um único Keycloak, permitir que todas consumam a mesma API e aplicar permissões diferentes de acordo com a aplicação que obteve o token.

## 1. Cenário do treinamento

Vamos trabalhar com quatro componentes:

```text
                               Keycloak
                                  |
                  +---------------+---------------+
                  |               |               |
                  v               v               v
             portal-ui      backoffice-ui    parceiro-job
                  |               |               |
                  +---------------+---------------+
                                  |
                                  v
                          pagamentos-api
```

Todas as origens acessam a mesma API:

```text
pagamentos-api
```

Mas possuem permissões diferentes:

| Origem | Tipo | Consultar | Criar | Cancelar |
|---|---|---:|---:|---:|
| `portal-ui` | Usuário / OIDC | ✅ | ✅ | ❌ |
| `backoffice-ui` | Usuário / OIDC | ✅ | ✅ | ✅ |
| `parceiro-job` | Sistema / Client Credentials | ❌ | ✅ | ❌ |

O problema que queremos resolver é:

> Como a API identifica qual aplicação obteve o token e como garante que cada origem execute somente as operações permitidas?

---

# 2. Conceitos fundamentais

## 2.1 Autenticação

**Authentication / Autenticação** responde:

> Quem está realizando a chamada?

Para um usuário:

```text
sub = identificador do usuário
```

Para uma aplicação usando Client Credentials:

```text
azp = identificador do client
```

---

## 2.2 Autorização

**Authorization / Autorização** responde:

> O chamador pode executar esta operação?

Exemplo:

```text
pagamento:read
pagamento:create
pagamento:cancel
```

A API não deve tomar a decisão apenas porque o usuário está autenticado.

---

## 2.3 Client

No Keycloak, cada aplicação que inicia autenticação ou solicita tokens deve ser representada por um **Client**.

Neste treinamento:

```text
portal-ui
backoffice-ui
parceiro-job
pagamentos-api
```

Não devemos usar um único Client para todas as aplicações se precisamos diferenciar as origens.

### Evitar

```text
client = sistema
```

usado simultaneamente por:

```text
Portal
Backoffice
Mobile
Parceiro
Integração
```

Nesse modelo, perdemos uma informação importante de segurança: qual aplicação recebeu/solicitou o token.

### Preferir

```text
portal-ui
backoffice-ui
mobile-app
parceiro-job
```

Cada origem possui sua própria identidade OAuth/OIDC.

---

# 3. Claims importantes

Uma **claim** é uma informação declarada dentro do token.

As quatro informações mais importantes para este cenário são:

| Claim | Significado | Exemplo |
|---|---|---|
| `sub` | Subject — identidade do usuário | `4cb7...` |
| `azp` | Authorized Party — client autorizado | `backoffice-ui` |
| `aud` | Audience — recurso/API destinatária | `pagamentos-api` |
| roles/permissões | Operações permitidas | `pagamento:cancel` |

Exemplo conceitual de token:

```json
{
  "iss": "http://localhost:8080/realms/treinamento",
  "sub": "76a6c8d2-...",
  "azp": "backoffice-ui",
  "aud": "pagamentos-api",
  "resource_access": {
    "pagamentos-api": {
      "roles": [
        "pagamento:read",
        "pagamento:create",
        "pagamento:cancel"
      ]
    }
  }
}
```

---

# 4. `azp` não é a mesma coisa que `aud`

Essa diferença é essencial.

## `azp`

Identifica a **Authorized Party**, normalmente o Client que solicitou/recebeu o token naquele fluxo.

```text
azp = backoffice-ui
```

Podemos interpretá-lo neste cenário como:

```text
qual aplicação obteve este token?
```

## `aud`

Identifica o **recurso para o qual o token deve ser aceito**.

```text
aud = pagamentos-api
```

A relação esperada é:

```text
backoffice-ui
     |
     | solicita token
     v
  Keycloak
     |
     | access token
     |
     | azp = backoffice-ui
     | aud = pagamentos-api
     v
pagamentos-api
```

Portanto:

```text
azp -> quem obteve o token
aud -> onde o token pode ser usado
```

---

# 5. Observação importante sobre "origem"

`azp` não significa endereço IP, domínio HTTP, `Origin` header ou `Referer`.

Ele identifica o **cliente OAuth/OIDC autorizado**.

Não use como mecanismo principal de segurança:

```http
X-Origin: backoffice
```

ou:

```http
Origin: https://backoffice.exemplo.com
```

Esses headers não substituem uma identidade criptograficamente vinculada ao token.

A origem lógica da aplicação deve ser representada por um Client no provedor de identidade.

---

# 6. Requisitos funcionais

## RF01 — Centralização da autenticação

Todas as aplicações deverão utilizar o mesmo Keycloak como Authorization Server / Identity Provider.

## RF02 — Client independente por origem

Cada origem deverá possuir seu próprio `client_id`.

Exemplos:

```text
portal-ui
backoffice-ui
parceiro-job
```

## RF03 — API como audiência

Tokens destinados à API deverão possuir:

```text
aud = pagamentos-api
```

## RF04 — Identificação da aplicação

A API deverá ser capaz de identificar o Client que obteve o token através de:

```text
azp
```

## RF05 — Autorização por permissão

A autorização deverá ser orientada por permissões, por exemplo:

```text
pagamento:read
pagamento:create
pagamento:cancel
```

## RF06 — Permissões diferentes por origem

Cada origem deverá receber somente o conjunto de permissões necessário.

## RF07 — Recurso compartilhado

Todas as origens poderão consumir o mesmo endpoint físico.

Exemplo:

```http
POST /api/pagamentos
```

Não é necessário criar:

```text
/api/portal/pagamentos
/api/backoffice/pagamentos
/api/parceiro/pagamentos
```

apenas para controlar autorização.

## RF08 — Resposta HTTP correta

A API deverá diferenciar:

```text
401 Unauthorized
```

Token ausente, inválido, expirado, assinatura inválida, issuer inválido ou audience inválida.

e:

```text
403 Forbidden
```

Token válido, mas sem permissão para executar a operação.

---

# 7. Requisitos de segurança

A API deverá validar no mínimo:

```text
assinatura
issuer
audience
expiração
```

Também devemos:

- utilizar HTTPS em ambientes reais;
- usar Authorization Code + PKCE para aplicações públicas;
- usar Client Credentials para comunicação machine-to-machine;
- nunca armazenar `client_secret` em SPA, browser ou aplicativo onde o segredo não possa ser protegido;
- não confiar em `Origin`, `Referer` ou `X-Origin` para autorização;
- evitar colocar regras de negócio complexas diretamente no Keycloak;
- manter access tokens com duração reduzida;
- não registrar o JWT completo em logs;
- registrar apenas identificadores necessários para auditoria;
- utilizar princípio do menor privilégio;
- restringir os scopes/roles liberados para cada Client.

---

# 8. Requisitos técnicos do laboratório

O exemplo utiliza:

```text
Keycloak 26.7.x
.NET 10 / ASP.NET Core
Docker
```

Os conceitos também se aplicam a aplicações ASP.NET Core recentes.

Para desenvolvimento:

```bash
docker --version
dotnet --version
```

---

# 9. Subindo o Keycloak

Crie:

```text
docker-compose.yml
```

Conteúdo:

```yaml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:26.7.1
    command: start-dev

    environment:
      KC_BOOTSTRAP_ADMIN_USERNAME: admin
      KC_BOOTSTRAP_ADMIN_PASSWORD: admin

    ports:
      - "8080:8080"
```

Execute:

```bash
docker compose up -d
```

A interface administrativa estará disponível em:

```text
http://localhost:8080
```

Credenciais do laboratório:

```text
usuário: admin
senha: admin
```

> Nunca utilize essas credenciais em produção.

---

# 10. Criando o Realm

No Admin Console:

```text
Create Realm
```

Nome:

```text
treinamento
```

Estrutura esperada:

```text
Keycloak
└── Realm: treinamento
```

---

# 11. Criando a API como Client

Crie um Client:

```text
Client ID: pagamentos-api
Protocol: OpenID Connect
```

Esse Client representa o **Resource Server**, ou seja, a API que receberá os tokens.

Ele também será utilizado para organizar as roles específicas do recurso.

Estrutura:

```text
Realm: treinamento

Clients
└── pagamentos-api
```

---

# 12. Criando as permissões

Em:

```text
Clients
  -> pagamentos-api
  -> Roles
```

crie:

```text
pagamento:read
pagamento:create
pagamento:cancel
```

Resultado:

```text
pagamentos-api
└── Roles
    ├── pagamento:read
    ├── pagamento:create
    └── pagamento:cancel
```

Essas são **Client Roles**, e não Realm Roles.

Isso é útil porque as permissões pertencem especificamente ao recurso:

```text
pagamentos-api
```

---

# 13. Criando o `portal-ui`

Crie:

```text
Client ID: portal-ui
Client type: OpenID Connect
```

Para uma SPA:

```text
Client authentication: OFF
Standard flow: ON
```

Utilize Authorization Code + PKCE.

Configure URLs do laboratório conforme seu frontend, por exemplo:

```text
Valid redirect URIs:
http://localhost:5173/*

Web origins:
http://localhost:5173
```

> Em produção, use URLs exatas e HTTPS sempre que possível. Evite wildcards amplos.

---

# 14. Criando o `backoffice-ui`

Crie outro Client:

```text
Client ID: backoffice-ui
Client authentication: OFF
Standard flow: ON
```

Exemplo:

```text
Valid redirect URIs:
http://localhost:5174/*

Web origins:
http://localhost:5174
```

Mesmo que Portal e Backoffice usem exatamente a mesma tecnologia, deverão possuir Clients diferentes porque são origens lógicas diferentes.

---

# 15. Criando o `parceiro-job`

Para um processo backend sem usuário:

```text
Client ID: parceiro-job
Client authentication: ON
Service accounts roles: ON
```

Nesse caso utilizaremos:

```text
OAuth 2.0 Client Credentials
```

Fluxo:

```text
parceiro-job
     |
     | client_id + secret
     v
 Keycloak
     |
     | access_token
     v
pagamentos-api
```

---

# 16. Modelando as permissões por origem

Queremos:

```text
portal-ui
├── pagamento:read
└── pagamento:create
```

```text
backoffice-ui
├── pagamento:read
├── pagamento:create
└── pagamento:cancel
```

```text
parceiro-job
└── pagamento:create
```

Uma matriz útil:

| Client | Roles permitidas |
|---|---|
| `portal-ui` | `pagamento:read`, `pagamento:create` |
| `backoffice-ui` | `pagamento:read`, `pagamento:create`, `pagamento:cancel` |
| `parceiro-job` | `pagamento:create` |

---

# 17. Mesmo usuário, permissões diferentes dependendo da aplicação

Este é um dos pontos mais importantes do cenário.

Considere o usuário:

```text
joao
```

João pode acessar tanto:

```text
portal-ui
```

quanto:

```text
backoffice-ui
```

Mas queremos:

```text
João pelo Portal
    -> read
    -> create
```

e:

```text
João pelo Backoffice
    -> read
    -> create
    -> cancel
```

Não basta simplesmente colocar todas as roles no token em qualquer Client.

Precisamos limitar o conjunto efetivo de roles que cada Client pode receber.

Para isso podemos usar:

```text
Client Scopes
+
Role Scope Mappings
+
Full Scope Allowed = OFF
```

---

# 18. Criando Client Scopes por contexto de acesso

Crie dois Client Scopes:

```text
pagamentos-portal
pagamentos-backoffice
```

Conceitualmente:

```text
pagamentos-portal
└── pagamentos-api
    ├── pagamento:read
    └── pagamento:create
```

```text
pagamentos-backoffice
└── pagamentos-api
    ├── pagamento:read
    ├── pagamento:create
    └── pagamento:cancel
```

Associe:

```text
portal-ui
└── pagamentos-portal
```

e:

```text
backoffice-ui
└── pagamentos-backoffice
```

Configure os Role Scope Mappings para que cada escopo exponha somente as roles permitidas.

Também desabilite o acesso amplo às roles:

```text
Full Scope Allowed = OFF
```

nos Clients de origem quando a estratégia utilizada depender de escopos restritos.

A ideia é aplicar:

```text
roles do usuário
        INTERSEÇÃO
roles permitidas para o client/scope
        =
roles efetivas no token
```

---

# 19. Adicionando `pagamentos-api` ao `aud`

A API deve validar:

```text
aud = pagamentos-api
```

No Keycloak, a audiência pode ser resolvida a partir de Client Scopes e Client Roles.

Para deixar o laboratório explícito e previsível, podemos configurar um **Audience Mapper** no Client Scope usado para acessar a API.

Conceito:

```text
Client Scope
└── Mapper
    └── Audience
        └── Included Client Audience = pagamentos-api
```

O token esperado passa a conter algo semelhante a:

```json
{
  "aud": "pagamentos-api"
}
```

ou:

```json
{
  "aud": [
    "pagamentos-api"
  ]
}
```

A API deve aceitar somente tokens cuja audiência inclua:

```text
pagamentos-api
```

---

# 20. Token esperado do Portal

Exemplo:

```json
{
  "sub": "usuario-123",
  "azp": "portal-ui",
  "aud": "pagamentos-api",
  "resource_access": {
    "pagamentos-api": {
      "roles": [
        "pagamento:read",
        "pagamento:create"
      ]
    }
  }
}
```

Observe:

```text
azp = portal-ui
```

e não:

```text
azp = pagamentos-api
```

A API é o destino:

```text
aud = pagamentos-api
```

---

# 21. Token esperado do Backoffice

```json
{
  "sub": "usuario-123",
  "azp": "backoffice-ui",
  "aud": "pagamentos-api",
  "resource_access": {
    "pagamentos-api": {
      "roles": [
        "pagamento:read",
        "pagamento:create",
        "pagamento:cancel"
      ]
    }
  }
}
```

O mesmo usuário pode aparecer em ambos.

O que muda é o Client:

```text
azp
```

e o conjunto efetivo de permissões.

---

# 22. Token esperado do Parceiro

Com Client Credentials, não existe necessariamente um usuário humano.

Exemplo conceitual:

```json
{
  "sub": "service-account-parceiro-job",
  "azp": "parceiro-job",
  "aud": "pagamentos-api",
  "resource_access": {
    "pagamentos-api": {
      "roles": [
        "pagamento:create"
      ]
    }
  }
}
```

---

# 23. Criando a API ASP.NET Core

Crie o projeto:

```bash
dotnet new webapi \
  --name Pagamentos.Api \
  --framework net10.0
```

Entre no diretório:

```bash
cd Pagamentos.Api
```

Adicione autenticação JWT Bearer:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

---

# 24. Configuração

`appsettings.Development.json`:

```json
{
  "Authentication": {
    "Authority": "http://localhost:8080/realms/treinamento",
    "Audience": "pagamentos-api"
  }
}
```

Em produção:

```text
Authority deve utilizar HTTPS.
```

---

# 25. Configurando JWT Bearer

`Program.cs`:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Authentication:Authority"]!;
var audience = builder.Configuration["Authentication:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;

        // Somente para o laboratório local usando HTTP.
        options.RequireHttpsMetadata = false;

        // Mantém os nomes originais das claims do JWT,
        // como "sub", "azp" e "aud".
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

O middleware deverá validar o token antes que a requisição chegue ao endpoint protegido.

---

# 26. Primeiro teste: endpoint apenas autenticado

Adicione:

```csharp
app.MapGet("/api/me", (HttpContext context) =>
{
    var subject = context.User.FindFirst("sub")?.Value;
    var client = context.User.FindFirst("azp")?.Value;

    return Results.Ok(new
    {
        subject,
        client
    });
})
.RequireAuthorization();
```

Resultado esperado para Backoffice:

```json
{
  "subject": "usuario-123",
  "client": "backoffice-ui"
}
```

Aqui já conseguimos identificar a origem lógica:

```csharp
context.User.FindFirst("azp")?.Value
```

---

# 27. Problema: roles do Keycloak são estruturadas

Por padrão, as Client Roles podem chegar em:

```json
{
  "resource_access": {
    "pagamentos-api": {
      "roles": [
        "pagamento:read",
        "pagamento:create"
      ]
    }
  }
}
```

O ASP.NET Core não deve depender de controllers lendo esse JSON manualmente em todo endpoint.

Evite:

```csharp
if (token.resource_access....)
{
}
```

em cada Controller.

Vamos normalizar as roles para claims internas:

```text
permission = pagamento:read
permission = pagamento:create
```

---

# 28. Transformando roles em `permission`

Crie:

```text
Security/KeycloakRolesClaimsTransformation.cs
```

```csharp
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Pagamentos.Api.Security;

public sealed class KeycloakRolesClaimsTransformation
    : IClaimsTransformation
{
    private const string ApiClientId = "pagamentos-api";

    public Task<ClaimsPrincipal> TransformAsync(
        ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
            return Task.FromResult(principal);

        // IClaimsTransformation pode ser executado mais de uma vez.
        // Não adicionamos as mesmas permissões novamente.
        var existingPermissions = identity
            .FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet(StringComparer.Ordinal);

        var resourceAccessClaim =
            identity.FindFirst("resource_access");

        if (resourceAccessClaim is null)
            return Task.FromResult(principal);

        using var document =
            JsonDocument.Parse(resourceAccessClaim.Value);

        if (!document.RootElement.TryGetProperty(
                ApiClientId,
                out var apiAccess))
        {
            return Task.FromResult(principal);
        }

        if (!apiAccess.TryGetProperty(
                "roles",
                out var roles))
        {
            return Task.FromResult(principal);
        }

        foreach (var role in roles.EnumerateArray())
        {
            var permission = role.GetString();

            if (string.IsNullOrWhiteSpace(permission))
                continue;

            if (!existingPermissions.Add(permission))
                continue;

            identity.AddClaim(
                new Claim("permission", permission));
        }

        return Task.FromResult(principal);
    }
}
```

Registre:

```csharp
builder.Services.AddTransient<
    IClaimsTransformation,
    KeycloakRolesClaimsTransformation>();
```

Adicione os namespaces:

```csharp
using Microsoft.AspNetCore.Authentication;
using Pagamentos.Api.Security;
```

Agora internamente teremos:

```text
permission = pagamento:read
permission = pagamento:create
```

---

# 29. Criando policies

Em vez de verificar permissões manualmente no endpoint:

```csharp
if (...)
```

configure Policies.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "PagamentoRead",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
                "permission",
                "pagamento:read");
        });

    options.AddPolicy(
        "PagamentoCreate",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
                "permission",
                "pagamento:create");
        });

    options.AddPolicy(
        "PagamentoCancel",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
                "permission",
                "pagamento:cancel");
        });
});
```

---

# 30. Protegendo os endpoints

```csharp
app.MapGet(
    "/api/pagamentos",
    () => Results.Ok("Consulta permitida"))
.RequireAuthorization("PagamentoRead");
```

```csharp
app.MapPost(
    "/api/pagamentos",
    () => Results.Ok("Pagamento criado"))
.RequireAuthorization("PagamentoCreate");
```

```csharp
app.MapDelete(
    "/api/pagamentos/{id:guid}",
    (Guid id) => Results.NoContent())
.RequireAuthorization("PagamentoCancel");
```

Agora os endpoints são independentes da aplicação de origem.

A regra é:

```text
endpoint
   |
   v
permissão necessária
```

e não:

```text
endpoint
   |
   v
if portal
else if backoffice
else if parceiro
```

---

# 31. Quando verificar também o `azp`

Na maior parte das operações, autorizar pela permissão é mais flexível.

Exemplo:

```text
DELETE /pagamentos/{id}
```

exige:

```text
pagamento:cancel
```

Se somente o Backoffice recebe essa permissão, já temos isolamento.

Entretanto, alguns requisitos podem dizer explicitamente:

> Mesmo que outro Client obtenha `pagamento:cancel`, essa operação só pode ser executada pelo Backoffice.

Nesse caso, `azp` faz parte da política.

Exemplo:

```csharp
options.AddPolicy(
    "PagamentoCancelBackoffice",
    policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireClaim(
            "azp",
            "backoffice-ui");

        policy.RequireClaim(
            "permission",
            "pagamento:cancel");
    });
```

Agora são necessárias duas condições:

```text
azp = backoffice-ui
AND
permission = pagamento:cancel
```

---

# 32. Separação recomendada de responsabilidades

## Keycloak

Responsável por:

```text
identidade
login
sessão
MFA
emissão de tokens
client_id
scopes
roles
audience
```

## ASP.NET Core

Responsável por:

```text
validação do token
authorization policies
regras específicas da API
regras contextuais de negócio
```

Exemplo:

Keycloak informa:

```text
permission = pagamento:cancel
```

A aplicação ainda pode validar:

```text
usuário pertence à empresa do pagamento?
pagamento está em situação cancelável?
janela de cancelamento ainda está aberta?
```

Essas regras não precisam virar roles do Keycloak.

---

# 33. Não transformar toda regra de negócio em claim

Evite tokens como:

```json
{
  "pode_cancelar_pagamento_da_filial_123_se_valor_menor_5000": true
}
```

Claims devem representar fatos ou concessões relativamente estáveis.

A API deve continuar responsável por decisões que dependem do recurso atual.

Exemplo:

```text
Claim:
permission = pagamento:cancel

Regra da aplicação:
pagamento.Status == Pendente
AND
pagamento.EmpresaId == usuario.EmpresaId
```

---

# 34. Machine-to-machine com Client Credentials

Para `parceiro-job`, obtenha o token através do endpoint:

```text
POST
/realms/treinamento/protocol/openid-connect/token
```

Exemplo:

```bash
curl \
  --request POST \
  "http://localhost:8080/realms/treinamento/protocol/openid-connect/token" \
  --header "Content-Type: application/x-www-form-urlencoded" \
  --data-urlencode "grant_type=client_credentials" \
  --data-urlencode "client_id=parceiro-job" \
  --data-urlencode "client_secret=SEU_SECRET"
```

Resposta:

```json
{
  "access_token": "eyJ...",
  "expires_in": 300,
  "token_type": "Bearer"
}
```

Use:

```bash
curl \
  "http://localhost:5000/api/pagamentos" \
  --header "Authorization: Bearer SEU_TOKEN"
```

---

# 35. Service Account Roles

Para Client Credentials, as permissões pertencem à identidade do próprio Client.

No Keycloak:

```text
parceiro-job
└── Service Account
    └── Role mappings
        └── pagamentos-api
            └── pagamento:create
```

Resultado esperado:

```text
parceiro-job
    |
    | Client Credentials
    v
Keycloak
    |
    | azp = parceiro-job
    | aud = pagamentos-api
    | role = pagamento:create
    v
pagamentos-api
```

---

# 36. Fluxo para usuário

Para Portal/Backoffice, o fluxo recomendado é:

```text
Authorization Code + PKCE
```

Fluxo simplificado:

```text
Browser
   |
   v
portal-ui
   |
   | redirect
   v
Keycloak
   |
   | login
   v
Authorization Code
   |
   | PKCE
   v
Access Token
   |
   v
pagamentos-api
```

O frontend envia:

```http
Authorization: Bearer <access_token>
```

---

# 37. O mesmo usuário em aplicações diferentes

Exemplo:

```text
João
```

Login via Portal:

```json
{
  "sub": "joao-id",
  "azp": "portal-ui",
  "aud": "pagamentos-api"
}
```

Login via Backoffice:

```json
{
  "sub": "joao-id",
  "azp": "backoffice-ui",
  "aud": "pagamentos-api"
}
```

Observe:

```text
sub permanece igual
azp muda
```

Essa é exatamente a separação que queremos.

---

# 38. SSO não elimina a identificação do Client

Mesmo usando SSO:

```text
Portal
   |
   v
Keycloak
   |
   | usuário já autenticado
   v
Backoffice
```

cada aplicação continua sendo um Client distinto.

Portanto, podemos ter:

```text
portal-ui
backoffice-ui
```

compartilhando a sessão do Realm, mas obtendo tokens no contexto de Clients diferentes.

---

# 39. Validando a Audience no .NET

Esta configuração:

```csharp
options.Audience = "pagamentos-api";
```

faz com que a API espere que o token seja destinado ao recurso.

Um token como:

```json
{
  "aud": "outra-api"
}
```

não deve ser aceito pela:

```text
pagamentos-api
```

Resultado esperado:

```text
401 Unauthorized
```

---

# 40. Por que não usar apenas `azp`

Imagine:

```json
{
  "azp": "backoffice-ui",
  "aud": "relatorios-api"
}
```

Esse token pode ter sido emitido para outro recurso.

Se a Pagamentos API validar apenas:

```text
azp = backoffice-ui
```

ela corre o risco de aceitar um token que não foi destinado a ela.

Por isso validamos:

```text
issuer
+
signature
+
lifetime
+
audience
```

e utilizamos:

```text
azp
```

como informação adicional do Client.

---

# 41. Por que não usar apenas `aud`

Imagine dois Clients:

```text
portal-ui
backoffice-ui
```

Ambos acessam:

```text
pagamentos-api
```

Então ambos possuem:

```text
aud = pagamentos-api
```

O `aud` não informa sozinho qual aplicação obteve o token.

Precisamos de:

```text
azp
```

para essa informação.

---

# 42. Modelo mental final

Use este modelo:

```text
sub
 |
 +--> Quem é o usuário?

azp
 |
 +--> Qual Client obteve o token?

aud
 |
 +--> Para qual recurso o token pode ser usado?

permission
 |
 +--> O que o chamador pode fazer?
```

Exemplo:

```text
sub        = 123
azp        = backoffice-ui
aud        = pagamentos-api
permission = pagamento:cancel
```

---

# 43. Criando um contexto de origem na aplicação

Se várias partes da aplicação precisam conhecer o Client, não espalhe:

```csharp
User.FindFirst("azp")
```

por toda a solução.

Podemos encapsular.

```csharp
public interface ICallerContext
{
    string? Subject { get; }
    string? ClientId { get; }
}
```

Implementação:

```csharp
using System.Security.Claims;

public sealed class CallerContext(
    IHttpContextAccessor httpContextAccessor)
    : ICallerContext
{
    private ClaimsPrincipal? User =>
        httpContextAccessor.HttpContext?.User;

    public string? Subject =>
        User?.FindFirst("sub")?.Value;

    public string? ClientId =>
        User?.FindFirst("azp")?.Value;
}
```

Registro:

```csharp
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICallerContext,
    CallerContext>();
```

Uma classe de domínio/aplicação não precisa conhecer detalhes do `HttpContext`.

---

# 44. Auditoria

Para operações sensíveis, podemos registrar:

```text
timestamp
sub
azp
ação
recurso
resultado
correlation-id
```

Exemplo:

```json
{
  "sub": "usuario-123",
  "azp": "backoffice-ui",
  "acao": "pagamento:cancel",
  "pagamentoId": "f881...",
  "resultado": "permitido"
}
```

Evite registrar:

```text
access_token completo
refresh_token
client_secret
senha
```

---

# 45. Matriz de testes

## Teste 1 — Sem token

```http
GET /api/pagamentos
```

Esperado:

```text
401
```

---

## Teste 2 — Token válido do Portal para leitura

Token:

```text
azp = portal-ui
permission = pagamento:read
aud = pagamentos-api
```

Chamada:

```http
GET /api/pagamentos
```

Esperado:

```text
200
```

---

## Teste 3 — Portal tentando cancelar

Token:

```text
azp = portal-ui
permission = pagamento:read
permission = pagamento:create
```

Chamada:

```http
DELETE /api/pagamentos/{id}
```

Esperado:

```text
403
```

---

## Teste 4 — Backoffice cancelando

Token:

```text
azp = backoffice-ui
permission = pagamento:cancel
aud = pagamentos-api
```

Esperado:

```text
200 ou 204
```

---

## Teste 5 — Parceiro criando

Token:

```text
azp = parceiro-job
permission = pagamento:create
```

Chamada:

```http
POST /api/pagamentos
```

Esperado:

```text
200/201
```

---

## Teste 6 — Parceiro cancelando

Esperado:

```text
403
```

---

## Teste 7 — Audience incorreta

Token:

```text
aud = relatorios-api
```

Enviado para:

```text
pagamentos-api
```

Esperado:

```text
401
```

---

## Teste 8 — Token expirado

Esperado:

```text
401
```

---

## Teste 9 — Tentativa de falsificar origem

Token:

```text
azp = portal-ui
```

Header adicionado manualmente:

```http
X-Origin: backoffice-ui
```

Esperado:

```text
continua sendo portal-ui
```

A autorização deve ignorar o header falso.

---

# 46. 401 vs 403

Regra prática:

```text
Não consegui estabelecer uma identidade/token válido
    -> 401
```

```text
Identidade/token é válido, mas não possui permissão
    -> 403
```

Exemplo:

```text
token expirado
-> 401

token válido sem pagamento:cancel
-> 403
```

---

# 47. Testando com Postman

Para aplicações de usuário, configure OAuth 2.0:

```text
Grant Type:
Authorization Code (With PKCE)

Auth URL:
http://localhost:8080/realms/treinamento/protocol/openid-connect/auth

Access Token URL:
http://localhost:8080/realms/treinamento/protocol/openid-connect/token
```

Para Portal:

```text
Client ID:
portal-ui
```

Para Backoffice:

```text
Client ID:
backoffice-ui
```

Compare os dois access tokens.

Procure:

```text
sub
azp
aud
resource_access
```

---

# 48. Inspecionando o JWT

Durante desenvolvimento você pode decodificar o token para inspecionar seu payload.

Nunca confunda:

```text
decodificar
```

com:

```text
validar
```

Qualquer pessoa que possua um JWT pode ler seu payload quando ele não está criptografado.

Quem garante autenticidade é a validação criptográfica feita pela API.

Portanto:

```text
jwt decodificado != jwt confiável
```

A API só deve confiar nele depois da validação.

---

# 49. Não enviar dados sensíveis no JWT

Evite colocar:

```text
senha
segredos
dados bancários sensíveis
informações que não precisam circular
```

O payload de um JWT assinado normalmente é legível por quem possui o token.

---

# 50. Alternativa: claim `permission` diretamente no Keycloak

No exemplo .NET fizemos:

```text
resource_access
    ->
IClaimsTransformation
    ->
permission
```

Outra possibilidade é configurar um **Protocol Mapper** no Keycloak que produza diretamente:

```json
{
  "permission": [
    "pagamento:read",
    "pagamento:create"
  ]
}
```

Então a API fica ainda mais simples:

```csharp
policy.RequireClaim(
    "permission",
    "pagamento:create");
```

Arquiteturalmente existem duas opções válidas:

### Opção A — Manter estrutura nativa

```text
Keycloak resource_access
        |
        v
.NET normaliza
```

Vantagem:

```text
menor customização no Keycloak
```

### Opção B — Normalizar no Keycloak

```text
Keycloak
        |
        v
permission[]
```

Vantagem:

```text
contrato de autorização mais simples para várias APIs
```

Para começar o treinamento, implemente primeiro a Opção A.

Depois implemente a Opção B como exercício.

---

# 51. Policy baseada apenas em permissão

Preferência padrão:

```csharp
policy.RequireClaim(
    "permission",
    "pagamento:cancel");
```

Isso mantém a API desacoplada do nome dos consumidores.

Se amanhã surgir:

```text
novo-backoffice
```

e ele receber:

```text
pagamento:cancel
```

o endpoint não precisa ser alterado.

---

# 52. Policy baseada em origem + permissão

Use quando o negócio explicitamente exige a identidade da aplicação:

```csharp
policy.RequireClaim(
    "azp",
    "backoffice-ui");

policy.RequireClaim(
    "permission",
    "pagamento:cancel");
```

Não faça isso indiscriminadamente.

Caso contrário a API começa a conhecer todos os consumidores:

```text
portal
mobile
backoffice
parceiro-a
parceiro-b
parceiro-c
...
```

e a autorização fica fortemente acoplada às origens.

---

# 53. Evolução: Authorization Handler

Quando a autorização depender de contexto dinâmico, uma Policy simples pode não ser suficiente.

Exemplo:

> Usuário pode cancelar um pagamento somente se tiver `pagamento:cancel` e pertencer à empresa responsável pelo pagamento.

Crie um `AuthorizationRequirement`.

```csharp
using Microsoft.AspNetCore.Authorization;

public sealed class CancelarPagamentoRequirement
    : IAuthorizationRequirement;
```

Handler:

```csharp
using Microsoft.AspNetCore.Authorization;

public sealed class CancelarPagamentoHandler
    : AuthorizationHandler<CancelarPagamentoRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CancelarPagamentoRequirement requirement)
    {
        var possuiPermissao =
            context.User.HasClaim(
                "permission",
                "pagamento:cancel");

        if (possuiPermissao)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

Isso pode evoluir para consultar informações adicionais do recurso.

---

# 54. Não confundir Client Role com Realm Role

## Realm Role

Representa algo global no Realm.

Exemplo:

```text
administrador-global
```

## Client Role

Representa uma permissão relacionada a uma aplicação/recurso.

Exemplo:

```text
pagamentos-api/pagamento:cancel
```

Para este cenário, Client Roles são uma boa opção para permissões específicas da API.

---

# 55. Groups

Se houver muitos usuários, não é necessário atribuir roles individualmente.

Podemos utilizar:

```text
Groups
```

Exemplo:

```text
Grupo: operadores-backoffice
└── roles
    ├── pagamento:read
    ├── pagamento:create
    └── pagamento:cancel
```

Usuários:

```text
Maria
João
Pedro
```

entram no grupo.

Isso facilita administração de identidade.

A restrição por Client Scope continua sendo importante quando a mesma pessoa utiliza aplicações com contextos diferentes.

---

# 56. SSO

Com um Realm compartilhado:

```text
portal-ui
backoffice-ui
```

podem aproveitar a sessão de autenticação do Keycloak.

Fluxo:

```text
Usuário abre Portal
     |
     v
Keycloak
     |
     | login
     v
Portal
```

Depois:

```text
Usuário abre Backoffice
     |
     v
Keycloak
     |
     | sessão já existente
     v
Backoffice
```

Isso é:

```text
Single Sign-On / SSO
```

Mas os tokens continuam vinculados ao contexto dos respectivos Clients.

---

# 57. Quando criar vários Realms

Não crie um Realm por aplicação apenas para diferenciar permissões.

Evite:

```text
realm-portal
realm-backoffice
realm-mobile
```

se todos compartilham o mesmo domínio de identidade.

Vários Realms fazem mais sentido quando existe isolamento forte, por exemplo:

```text
realm-funcionarios
realm-clientes
realm-parceiros
```

com ciclos de identidade e políticas realmente independentes.

---

# 58. Cenário com API Gateway

Se existir:

```text
Frontend
   |
   v
API Gateway
   |
   v
pagamentos-api
```

a API ainda deve validar um token apropriado.

Não considere automaticamente:

```text
IP do Gateway
```

como autorização do usuário.

A identidade precisa ser propagada de forma segura.

---


# 58.1 Como a aplicação sabe qual Realm utilizar?

O Keycloak **não descobre automaticamente o Realm a partir do usuário**.

A aplicação que inicia o fluxo de autenticação já precisa estar configurada para utilizar um Realm específico.

Por exemplo:

```text
Aplicação: portal-ui

Authority:
https://auth.exemplo.com/realms/empresa

ClientId:
portal-ui
```

Isso significa:

```text
Keycloak
└── Realm: empresa
    └── Client: portal-ui
```

Quando a aplicação inicia o login, ela envia o usuário para um endpoint que já contém o Realm na URL.

Exemplo:

```text
https://auth.exemplo.com
    /realms/empresa
    /protocol/openid-connect/auth
```

A requisição de autenticação também informa o Client:

```text
client_id=portal-ui
```

Portanto, o Keycloak recebe duas informações diferentes:

```text
URL
 ↓
Realm = empresa

Query String
 ↓
client_id = portal-ui
```

Visualmente:

```text
portal-ui
    |
    | Authority =
    | https://auth.exemplo.com/realms/empresa
    |
    | ClientId = portal-ui
    v
Keycloak
    |
    v
Realm: empresa
    |
    v
Client: portal-ui
    |
    v
Authentication Flow
    |
    v
JWT
```

O Realm é escolhido **antes da autenticação do usuário começar**.

---

# 58.2 Endpoint OIDC por Realm

Cada Realm possui seus próprios endpoints OpenID Connect.

Exemplo:

```text
Realm: empresa
```

Authorization Endpoint:

```text
https://auth.exemplo.com/realms/empresa/protocol/openid-connect/auth
```

Token Endpoint:

```text
https://auth.exemplo.com/realms/empresa/protocol/openid-connect/token
```

Discovery Endpoint:

```text
https://auth.exemplo.com/realms/empresa/.well-known/openid-configuration
```

Outro Realm:

```text
Realm: clientes
```

possui endpoints diferentes:

```text
https://auth.exemplo.com/realms/clientes/protocol/openid-connect/auth
```

```text
https://auth.exemplo.com/realms/clientes/protocol/openid-connect/token
```

```text
https://auth.exemplo.com/realms/clientes/.well-known/openid-configuration
```

Portanto:

```text
Realm diferente
    ↓
Issuer diferente
    ↓
Endpoints OIDC diferentes
```

---

# 58.3 Relação entre `Authority`, Realm e `iss`

Na aplicação .NET podemos ter:

```json
{
  "Authentication": {
    "Authority": "https://auth.exemplo.com/realms/empresa",
    "Audience": "pagamentos-api"
  }
}
```

O `Authority` informa qual emissor OIDC a aplicação/API deve confiar.

Um token emitido por esse Realm normalmente possui:

```json
{
  "iss": "https://auth.exemplo.com/realms/empresa"
}
```

Portanto:

```text
Authority
    ↓
configuração da aplicação

iss
    ↓
informação recebida no token
```

Os dois precisam ser compatíveis.

Modelo mental:

```text
Aplicação confia em:
https://auth.exemplo.com/realms/empresa
                    |
                    v
                  Realm
                    |
                    v
Token:
iss = https://auth.exemplo.com/realms/empresa
```

---

# 58.4 Relação entre Realm e `azp`

O Realm não fica armazenado dentro do `azp`.

Exemplo:

```json
{
  "iss": "https://auth.exemplo.com/realms/empresa",
  "azp": "portal-ui"
}
```

Aqui:

```text
iss
→ identifica o emissor / Realm

azp
→ identifica o Client autorizado dentro daquele contexto
```

Podemos visualizar:

```text
Realm: empresa
    |
    +-- Client: portal-ui
            |
            +-- azp = portal-ui
```

Portanto:

```text
Realm ≠ azp
```

A relação acontece porque o Client existe dentro de um Realm.

---

# 58.5 Mesmo `client_id` em Realms diferentes

É possível possuir:

```text
Realm: empresa
└── Client: portal-ui
```

e:

```text
Realm: clientes
└── Client: portal-ui
```

Os dois podem utilizar:

```text
client_id = portal-ui
```

porque cada Realm possui seu próprio conjunto isolado de Clients.

Porém, olhando apenas:

```json
{
  "azp": "portal-ui"
}
```

não conseguimos saber de qual Realm o token veio.

Precisamos analisar também:

```text
iss
```

Exemplo 1:

```json
{
  "iss": "https://auth.exemplo.com/realms/empresa",
  "azp": "portal-ui"
}
```

Exemplo 2:

```json
{
  "iss": "https://auth.exemplo.com/realms/clientes",
  "azp": "portal-ui"
}
```

Apesar de:

```text
azp = portal-ui
```

ser igual nos dois casos, os contextos de segurança são diferentes.

Podemos pensar na identidade lógica do Client como:

```text
issuer + client_id
```

ou, observando o token:

```text
iss + azp
```

Exemplo:

```text
/realms/empresa + portal-ui
```

é diferente de:

```text
/realms/clientes + portal-ui
```

---

# 58.6 O Keycloak não procura o usuário em todos os Realms

Considere:

```text
Realm: empresa
└── joao@email.com
```

e:

```text
Realm: clientes
└── joao@email.com
```

O Keycloak não recebe:

```text
joao@email.com
```

e depois procura automaticamente:

```text
empresa?
clientes?
parceiros?
```

O fluxo real é:

```text
portal-ui
    |
    | chama
    v
/realms/empresa/...
    |
    v
Keycloak procura João
APENAS dentro do Realm empresa
```

Se outra aplicação chamar:

```text
/realms/clientes/...
```

o Keycloak procura o usuário dentro de:

```text
Realm: clientes
```

Portanto:

```text
❌ usuário escolhe o Realm implicitamente

✅ aplicação escolhe o Realm através do endpoint/Authority
```

---

# 58.7 Exemplo completo com dois Realms

Considere:

```text
Keycloak
├── Realm: empresa
│   ├── portal-ui
│   └── pagamentos-api
│
└── Realm: clientes
    ├── portal-ui
    └── pagamentos-api
```

A aplicação corporativa possui:

```text
Authority =
https://auth.exemplo.com/realms/empresa

ClientId =
portal-ui
```

O token pode ser:

```json
{
  "iss": "https://auth.exemplo.com/realms/empresa",
  "azp": "portal-ui",
  "aud": "pagamentos-api",
  "sub": "usuario-123"
}
```

A aplicação de clientes possui:

```text
Authority =
https://auth.exemplo.com/realms/clientes

ClientId =
portal-ui
```

O token pode ser:

```json
{
  "iss": "https://auth.exemplo.com/realms/clientes",
  "azp": "portal-ui",
  "aud": "pagamentos-api",
  "sub": "usuario-987"
}
```

Os dois tokens possuem:

```text
azp = portal-ui
aud = pagamentos-api
```

mas foram emitidos por autoridades diferentes:

```text
iss diferente
```

---

# 58.8 API aceitando apenas um Realm

Esse é o cenário mais simples.

Configuração:

```csharp
options.Authority =
    "https://auth.exemplo.com/realms/empresa";

options.Audience =
    "pagamentos-api";
```

A API passa a confiar no Realm:

```text
empresa
```

Nesse contexto, quando ela recebe:

```text
azp = portal-ui
```

o significado é:

```text
portal-ui dentro do Realm empresa
```

A API não precisa descobrir o Realm dinamicamente.

Ele já faz parte da configuração de segurança.

---

# 58.9 API aceitando vários Realms

Esse cenário exige mais cuidado.

Exemplo:

```text
pagamentos-api
    ↑
    |
    +-- Realm empresa
    |
    +-- Realm clientes
    |
    +-- Realm parceiros
```

Nesse caso, não devemos identificar o Client somente por:

```text
azp
```

Precisamos considerar:

```text
iss + azp
```

Exemplo:

```text
iss = /realms/empresa
azp = portal-ui
```

não é necessariamente equivalente a:

```text
iss = /realms/clientes
azp = portal-ui
```

Além disso, a API precisa estar explicitamente configurada para confiar em todos os issuers aceitos.

Não desabilite a validação de issuer apenas para facilitar esse cenário.

---

# 58.10 Como decidir se devemos criar outro Realm?

Pergunte:

> Este grupo precisa de um domínio de identidade realmente isolado?

Motivos válidos podem incluir:

```text
usuários completamente diferentes
políticas de senha diferentes
MFA diferente
Identity Providers diferentes
administração independente
ciclo de vida independente
isolamento organizacional forte
```

Por exemplo:

```text
Realm: funcionarios
Realm: clientes
Realm: parceiros
```

pode fazer sentido.

Mas isto:

```text
Realm: portal
Realm: backoffice
Realm: mobile
```

geralmente não faz sentido quando todas essas aplicações utilizam os mesmos usuários.

Nesse caso prefira:

```text
Realm: empresa
├── portal-ui
├── backoffice-ui
├── mobile-app
└── pagamentos-api
```

---

# 58.11 Resumo: quem informa cada coisa?

```text
Aplicação
    |
    +-- Authority
    |      ↓
    |    qual Realm?
    |
    +-- ClientId
           ↓
         qual Client?
```

Durante o login:

```text
https://auth.exemplo.com/realms/empresa/...
                                ↑
                              Realm
```

e:

```text
client_id=portal-ui
          ↑
        Client
```

Depois, no token:

```text
iss = https://auth.exemplo.com/realms/empresa
      ↑
      Realm / emissor

azp = portal-ui
      ↑
      Client autorizado

aud = pagamentos-api
      ↑
      recurso destinatário

sub = usuario-123
      ↑
      usuário
```

Modelo final:

```text
Configuração da aplicação
        |
        +--> Authority ------> Realm
        |
        +--> ClientId -------> Client
                                  |
                                  v
                               Keycloak
                                  |
                                  v
                                  JWT
                                  |
                    +-------------+-------------+
                    |             |             |
                    v             v             v
                   iss           azp           aud
                 Realm          Client         API
```


# 59. Cenário com BFF

Se utilizar **BFF — Backend for Frontend**:

```text
Browser
   |
   v
BFF
   |
   v
pagamentos-api
```

é possível que, dependendo do fluxo adotado, a API veja como Client autorizado o próprio BFF.

Portanto:

```text
azp
```

deve ser interpretado dentro do fluxo OAuth real adotado.

Não assuma que ele sempre representa a primeira interface visual que o usuário abriu.

---

# 60. Cenário com Token Exchange

Em arquiteturas como:

```text
Frontend
   |
   v
API A
   |
   | token exchange
   v
API B
```

o token recebido pela API B pode representar o Client que realizou o exchange.

Isso significa que:

```text
azp
```

não deve ser tratado como um "trace id da origem inicial".

Ele representa a parte autorizada no token atual.

Se for necessário transportar a origem inicial por uma cadeia de serviços, modele esse requisito explicitamente e com mecanismos confiáveis.

Não reutilize `azp` com uma semântica que o protocolo não garante.

---

# 61. Não usar claim customizada `origin` sem necessidade

Você poderia criar:

```json
{
  "origin": "backoffice"
}
```

Mas, se isso apenas replica:

```json
{
  "azp": "backoffice-ui"
}
```

estamos duplicando informação.

Comece usando a claim padrão existente.

Crie uma claim customizada somente quando existir uma semântica diferente.

Exemplo:

```text
channel = ecommerce
```

poderia agrupar:

```text
web-shop
mobile-shop
whatsapp-shop
```

Isso é diferente de identificar o `client_id`.

---

# 62. Modelo para múltiplos canais

Podemos separar:

```text
azp
```

de:

```text
channel
```

Exemplo:

```json
{
  "azp": "mobile-app",
  "channel": "digital",
  "aud": "pagamentos-api"
}
```

Outro:

```json
{
  "azp": "web-portal",
  "channel": "digital",
  "aud": "pagamentos-api"
}
```

Aqui:

```text
azp
-> aplicação específica

channel
-> classificação de negócio
```

Essa é uma situação em que uma claim adicional pode fazer sentido.

---

# 63. Permissões vs roles organizacionais

Tente diferenciar:

```text
cargo/role organizacional
```

de:

```text
permissão técnica da API
```

Exemplo organizacional:

```text
supervisor
gerente
analista
```

Permissões:

```text
pagamento:read
pagamento:create
pagamento:cancel
```

É possível usar roles compostas no Keycloak:

```text
supervisor
└── pagamento:read
└── pagamento:create
```

```text
gerente
└── pagamento:read
└── pagamento:create
└── pagamento:cancel
```

A API continua pensando em capacidades, não em cargos.

---

# 64. Princípio do menor privilégio

Uma aplicação deve receber apenas o que necessita.

Evite token:

```text
portal-ui
    -> todas as roles do usuário
```

Prefira:

```text
portal-ui
    -> apenas roles necessárias ao Portal
```

Isso reduz o impacto caso um access token seja comprometido.

---

# 65. Checklist do Keycloak

- [ ] Criar Realm `treinamento`
- [ ] Criar Client `pagamentos-api`
- [ ] Criar Client Role `pagamento:read`
- [ ] Criar Client Role `pagamento:create`
- [ ] Criar Client Role `pagamento:cancel`
- [ ] Criar Client `portal-ui`
- [ ] Criar Client `backoffice-ui`
- [ ] Criar Client `parceiro-job`
- [ ] Habilitar Standard Flow nos Clients de usuário
- [ ] Utilizar PKCE nos Clients públicos
- [ ] Habilitar Service Account no `parceiro-job`
- [ ] Criar Client Scope para Portal
- [ ] Criar Client Scope para Backoffice
- [ ] Restringir Role Scope Mappings
- [ ] Revisar `Full Scope Allowed`
- [ ] Garantir `aud = pagamentos-api`
- [ ] Inspecionar token do Portal
- [ ] Inspecionar token do Backoffice
- [ ] Inspecionar token do Parceiro
- [ ] Confirmar `azp` diferente em cada origem

---

# 66. Checklist da API .NET

- [ ] Instalar `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] Configurar `Authority`
- [ ] Configurar `Audience`
- [ ] Validar HTTPS em ambiente real
- [ ] Configurar `MapInboundClaims = false`
- [ ] Adicionar `UseAuthentication`
- [ ] Adicionar `UseAuthorization`
- [ ] Transformar Client Roles em `permission`
- [ ] Criar Policy de leitura
- [ ] Criar Policy de criação
- [ ] Criar Policy de cancelamento
- [ ] Proteger endpoints
- [ ] Testar token ausente
- [ ] Testar token expirado
- [ ] Testar audience incorreta
- [ ] Testar permissão insuficiente
- [ ] Testar cada `azp`
- [ ] Implementar auditoria sem registrar tokens

---

# 67. Exercício 1

Implemente:

```text
portal-ui
```

com:

```text
pagamento:read
```

Teste:

```http
GET /api/pagamentos
```

Esperado:

```text
200
```

Teste:

```http
POST /api/pagamentos
```

Esperado:

```text
403
```

---

# 68. Exercício 2

Adicione ao Portal:

```text
pagamento:create
```

Sem alterar a API.

Resultado esperado:

```text
POST /api/pagamentos
-> 200/201
```

Pergunta:

> Por que não foi necessário alterar o endpoint?

Resposta esperada:

> Porque o endpoint autoriza pela capacidade `pagamento:create`, e não pelo nome `portal-ui`.

---

# 69. Exercício 3

Configure:

```text
backoffice-ui
```

com:

```text
pagamento:cancel
```

Teste:

```text
Portal -> DELETE -> 403
Backoffice -> DELETE -> 204
```

---

# 70. Exercício 4

Crie:

```text
mobile-app
```

Permissões:

```text
pagamento:read
```

Sem alterar nenhum endpoint da API.

Token esperado:

```json
{
  "azp": "mobile-app",
  "aud": "pagamentos-api"
}
```

---

# 71. Exercício 5

Crie um token para:

```text
relatorios-api
```

e tente enviá-lo para:

```text
pagamentos-api
```

Resultado esperado:

```text
401
```

Objetivo:

> Comprovar que `aud` é uma fronteira de segurança diferente de `azp`.

---

# 72. Exercício 6

Envie um token do Portal e tente falsificar:

```http
X-Origin: backoffice-ui
```

O endpoint de cancelamento deve continuar retornando:

```text
403
```

Objetivo:

> Comprovar que a aplicação não confia em headers arbitrários para identidade/autorização.

---

# 73. Exercício 7 — Policy composta

Crie uma Policy:

```text
CancelamentoExclusivoBackoffice
```

Exija:

```text
azp = backoffice-ui
AND
permission = pagamento:cancel
```

Depois forneça `pagamento:cancel` temporariamente a outro Client.

Mesmo assim ele deverá receber:

```text
403
```

---

# 74. Exercício 8 — Regra contextual

Adicione:

```text
empresa_id
```

à identidade do usuário.

Implemente:

```text
usuário possui pagamento:cancel
AND
pagamento.EmpresaId == empresa_id do usuário
```

Objetivo:

> Entender a fronteira entre autorização concedida pelo IdP e autorização contextual da aplicação.

---

# 75. Exercício 9 — Auditoria

Para cada criação/cancelamento, registre:

```text
sub
azp
permission
recurso
resultado
correlation-id
```

Depois responda:

> Qual aplicação executou determinado cancelamento?

A resposta deve ser obtida pelos dados de auditoria, não por inferência de IP.

---

# 76. Estrutura de projeto sugerida

```text
Pagamentos.Api
│
├── Program.cs
│
├── appsettings.json
├── appsettings.Development.json
│
├── Security
│   ├── KeycloakRolesClaimsTransformation.cs
│   ├── ICallerContext.cs
│   ├── CallerContext.cs
│   └── Policies.cs
│
├── Endpoints
│   └── PagamentosEndpoints.cs
│
├── Application
│   └── ...
│
└── Domain
    └── ...
```

A camada de domínio não deve depender diretamente de Keycloak.

---

# 77. Constantes de permissões

Evite strings espalhadas:

```csharp
"pagamento:create"
```

Crie:

```csharp
public static class Permissions
{
    public const string PagamentoRead =
        "pagamento:read";

    public const string PagamentoCreate =
        "pagamento:create";

    public const string PagamentoCancel =
        "pagamento:cancel";
}
```

E:

```csharp
policy.RequireClaim(
    "permission",
    Permissions.PagamentoCancel);
```

---

# 78. Constantes de policies

```csharp
public static class Policies
{
    public const string PagamentoRead =
        nameof(PagamentoRead);

    public const string PagamentoCreate =
        nameof(PagamentoCreate);

    public const string PagamentoCancel =
        nameof(PagamentoCancel);
}
```

Uso:

```csharp
.RequireAuthorization(
    Policies.PagamentoCancel);
```

---

# 79. Contrato de segurança recomendado

Defina explicitamente o contrato esperado pela API:

```text
Issuer:
https://auth.exemplo.com/realms/empresa

Audience:
pagamentos-api

Client/origin:
azp

Permissions:
resource_access.pagamentos-api.roles
```

Internamente:

```text
resource_access
        |
        v
permission claim
```

Documentar esse contrato reduz ambiguidades entre as equipes.

---

# 80. Decisão arquitetural sugerida

Para este cenário:

```text
UM Realm
+
UM Client por origem
+
UM Client representando a API
+
Client Roles para permissões da API
+
Client Scopes / Scope Mappings para restringir cada origem
+
aud para validar o recurso
+
azp para identificar o Client
+
Policies .NET para autorizar operações
```

Visualmente:

```text
                           KEYCLOAK
                              |
            +-----------------+-----------------+
            |                 |                 |
            v                 v                 v
        portal-ui       backoffice-ui      parceiro-job
            |                 |                 |
            | azp=portal      | azp=backoffice  | azp=parceiro
            |                 |                 |
            +-----------------+-----------------+
                              |
                              | aud=pagamentos-api
                              v
                       PAGAMENTOS API
                              |
                    +---------+---------+
                    |         |         |
                    v         v         v
                   read     create    cancel
```

---

# 81. Perguntas para revisar depois do laboratório

1. Qual é a diferença entre autenticação e autorização?
2. O que representa um Realm?
3. Por que cada aplicação origem deve ser um Client diferente?
4. Qual é a diferença entre `sub`, `azp` e `aud`?
5. Por que não devemos usar `Origin` HTTP para autorização?
6. Por que a API deve validar `aud`?
7. Quando devemos verificar `azp` explicitamente?
8. Qual é a diferença entre Realm Role e Client Role?
9. Como restringir roles do mesmo usuário dependendo do Client?
10. O que significa `401`?
11. O que significa `403`?
12. Por que não devemos colocar todas as regras de negócio no Keycloak?
13. Quando Client Credentials é apropriado?
14. Por que um SPA não deve possuir `client_secret`?
15. O que muda quando existe Token Exchange?
16. `azp` representa obrigatoriamente a origem inicial de uma cadeia de microsserviços?
17. Por que permissões são geralmente melhores do que `if (origem == ...)`?
18. Qual a função de um Authorization Handler?
19. Como auditar a origem de uma operação sem registrar o JWT?
20. Quando uma claim customizada como `channel` faz sentido?

---

# 82. Resultado esperado do treinamento

Ao terminar, você deverá conseguir explicar e implementar:

```text
Autenticação centralizada
        |
        v
Keycloak
        |
        +--> identificação do usuário
        |
        +--> identificação do Client
        |
        +--> emissão de permissões
        |
        v
JWT
        |
        +--> sub
        +--> azp
        +--> aud
        +--> roles
        |
        v
ASP.NET Core
        |
        +--> valida token
        +--> valida audience
        +--> normaliza claims
        +--> executa policy
        |
        v
Regra de negócio
```

O princípio principal é:

> **A aplicação de origem é uma identidade OAuth diferente do recurso consumido. `azp` ajuda a identificar o Client autorizado, `aud` delimita o recurso destinatário e as permissões determinam o que pode ser executado.**

---

# 83. Próximos tópicos para estudo

Depois deste laboratório, evolua para:

```text
1. Refresh Token
2. Single Sign-On
3. Logout / Single Logout
4. MFA
5. Groups
6. Composite Roles
7. Authorization Handlers
8. API Gateway
9. BFF
10. Client Credentials
11. Token Exchange
12. Secret rotation
13. mTLS
14. DPoP
15. Observabilidade e auditoria
16. Federação de identidade
```

---

# 84. Referências oficiais

## Keycloak

Documentação de OpenID Connect:

```text
https://www.keycloak.org/securing-apps/oidc-layers
```

Planejamento para proteger aplicações e serviços:

```text
https://www.keycloak.org/securing-apps/overview
```

Server Administration Guide:

```text
https://www.keycloak.org/docs/latest/server_admin/
```

Authorization Services:

```text
https://www.keycloak.org/docs/latest/authorization_services/
```

Token Exchange:

```text
https://www.keycloak.org/securing-apps/token-exchange
```

## Microsoft / ASP.NET Core

JWT Bearer Authentication:

```text
https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication
```

Authentication:

```text
https://learn.microsoft.com/aspnet/core/security/authentication/
```

Policy-based Authorization:

```text
https://learn.microsoft.com/aspnet/core/security/authorization/policies
```

Claims-based Authorization:

```text
https://learn.microsoft.com/aspnet/core/security/authorization/claims
```

---

# 85. Resumo de bolso

```text
Quem é o usuário?
    -> sub

Qual aplicação obteve o token?
    -> azp

Para qual API o token foi destinado?
    -> aud

O que pode fazer?
    -> permission / client roles

Token inválido?
    -> 401

Token válido sem permissão?
    -> 403
```

Arquitetura:

```text
Origem A ----\
Origem B -----+--> Keycloak --> JWT --> Mesma API
Origem C ----/                   |
                                  +--> permissões diferentes
```

Regra:

```text
não autorize pelo nome da tela
autorize por capacidade
```

Exemplo:

```text
pagamento:read
pagamento:create
pagamento:cancel
```

Use `azp` quando a identidade do Client for, por si só, parte explícita do requisito de segurança.
