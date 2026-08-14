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
