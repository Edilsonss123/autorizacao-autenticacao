## ADDED Requirements

### Requirement: Identificação única do Pagamento
O domínio SHALL atribuir a cada `Pagamento` criado um identificador único (`Id`) do tipo `Guid`, gerado no momento da criação, permitindo que camadas externas localizem o agregado individualmente.

#### Scenario: Id atribuído na criação
- **WHEN** um `Pagamento` é criado com sucesso
- **THEN** o `Pagamento` possui um `Id` do tipo `Guid` diferente de `Guid.Empty`

#### Scenario: Ids distintos para pagamentos distintos
- **WHEN** dois `Pagamento` são criados em chamadas separadas
- **THEN** os dois `Pagamento` possuem `Id` distintos entre si
