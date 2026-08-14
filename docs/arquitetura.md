# Arquitetura

Este documento descreve a arquitetura do projeto. É referenciado pelo `AGENTS.md` e deve ser seguido por qualquer agente de IA ou desenvolvedor trabalhando no repositório.

## Hexagonal (Ports & Adapters) + DDD

- O **Domain** não depende de nenhuma outra camada (nem de ASP.NET Core, nem de Keycloak, nem de EF Core). Sem referências a frameworks de infraestrutura.
- **Application** orquestra casos de uso e define **ports** (interfaces) que a infraestrutura implementa. Não conhece detalhes de HTTP, banco ou Keycloak.
- **Infrastructure/Adapters** implementa os ports (repositórios, cliente Keycloak, persistência, etc.) e os **adapters de entrada** (endpoints HTTP via Minimal API).
- Dependências sempre apontam para dentro: `Infrastructure -> Application -> Domain`. Nunca o inverso.
- Regras de negócio pertencem ao Domain, não ao Keycloak nem aos Endpoints. Autorização técnica (claims/policies) fica na borda (Infrastructure); regras contextuais de negócio (ex.: `pagamento.EmpresaId == usuario.EmpresaId`) ficam na Application/Domain.

## Estrutura de pastas

Ver também `README.md`, seção "Estrutura de projeto sugerida".

```
src/
├── Domain/          # Entidades, Value Objects, regras de negócio puras
├── Application/      # Casos de uso, ports (interfaces), DTOs de aplicação
├── Infrastructure/   # Adapters: EF Core, Keycloak, repositórios
└── Api/              # Adapter de entrada: endpoints, Program.cs, policies HTTP
```

## API

- Usar sempre **Minimal API** para os endpoints HTTP. Nunca criar Controllers (`ControllerBase`, atributos `[ApiController]`/`[Route]`, etc.).
- Agrupar endpoints por capacidade em métodos de extensão (`MapGroup`, `Map...Endpoints`), mantendo `Program.cs` enxuto.
- Handlers de Minimal API não contêm lógica de negócio — apenas orquestração (parse de request, chamada ao caso de uso, mapeamento de resposta).

## Mapeamento

- Mapeamento entre camadas (request → comando/DTO, DTO → entidade de Domain, entidade → response) é sempre **manual** — construtores, factory methods ou métodos de mapeamento explícitos.
- Não usar bibliotecas de mapeamento automático (AutoMapper, Mapster, etc.). O mapeamento manual mantém a conversão visível e evita acoplamento implícito entre Domain e contratos de API.

## Validação

- Validação de entrada (requests, DTOs) usa **FluentValidation**.
- Validators ficam próximos ao adapter que os usa (ex.: junto aos endpoints em `Api/`), não no Domain — o Domain garante seus próprios invariantes através de suas entidades/Value Objects, independente de qualquer biblioteca de validação.

## Keycloak

A configuração do Realm `pagamentos` (Clients, Client Roles, Client Scopes, Audience Mapper, usuário e grupo de teste) é declarativa e versionada em `containers/config/keycloak/realm-export.json`. O `containers/docker-compose.yml` importa esse arquivo automaticamente (`start-dev --import-realm`), então `docker compose -f containers/docker-compose.yml up -d` já sobe com o Realm pronto, sem passos manuais no Admin Console.

O Admin Console (`http://localhost:7050`, usuário/senha `admin`/`admin` em ambiente de laboratório) pode ser usado para inspecionar ou ajustar a configuração, mas **o arquivo versionado é a fonte de verdade**. Qualquer alteração feita pelo Admin Console precisa ser refletida de volta no `realm-export.json` antes de ser considerada concluída.

### Como reexportar o Realm após uma alteração manual

O `kc.sh export` não roda com o servidor ativo (ele acessa o banco H2 diretamente), então:

```bash
# 1. Parar o Keycloak (a alteração feita via Admin Console já está persistida no volume)
docker compose -f containers/docker-compose.yml stop keycloak

# 2. Exportar o realm 'pagamentos' usando a mesma imagem, contra o mesmo volume de dados
docker run --rm \
  -v "$(pwd)/containers/data/keycloak:/opt/keycloak/data" \
  -v "$(pwd)/containers/keycloak-export:/tmp/kc-export" \
  quay.io/keycloak/keycloak:26.7.1 \
  export --dir /tmp/kc-export --realm pagamentos --users realm_file

# 3. Substituir o arquivo versionado e limpar o diretório temporário
cp containers/keycloak-export/pagamentos-realm.json containers/config/keycloak/realm-export.json
rm -rf containers/keycloak-export

# 4. Subir o Keycloak novamente
docker compose -f containers/docker-compose.yml up -d
```

Segredos de Client (`parceiro-job`) e a senha do usuário de teste (`joao`) usados no laboratório são valores fixos de desenvolvimento, nunca reaproveitáveis em produção.
