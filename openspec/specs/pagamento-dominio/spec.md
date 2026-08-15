# pagamento-dominio Specification

## Purpose

Garante que o agregado `Pagamento` só existe em estados válidos e que suas transições (criação, cancelamento) respeitam as invariantes de negócio, independentemente de qualquer camada externa (Application, Infrastructure, Api).

## Requirements

### Requirement: Criação de um Pagamento válido
O domínio SHALL permitir criar um `Pagamento` informando um valor monetário positivo, resultando em um agregado com `Status = Pendente`.

#### Scenario: Criação com valor positivo
- **WHEN** um `Pagamento` é criado com um valor monetário maior que zero
- **THEN** o `Pagamento` é criado com sucesso e seu `Status` é `Pendente`

### Requirement: Valor monetário inválido é rejeitado na criação
O domínio SHALL rejeitar a criação de um `Pagamento` cujo valor monetário seja zero ou negativo, lançando uma exceção de domínio.

#### Scenario: Tentativa de criação com valor zero
- **WHEN** um `Pagamento` é criado com valor monetário igual a zero
- **THEN** uma exceção de domínio é lançada e nenhum `Pagamento` é criado

#### Scenario: Tentativa de criação com valor negativo
- **WHEN** um `Pagamento` é criado com valor monetário negativo
- **THEN** uma exceção de domínio é lançada e nenhum `Pagamento` é criado

### Requirement: Cancelamento de um Pagamento Pendente
O domínio SHALL permitir cancelar um `Pagamento` cujo `Status` seja `Pendente`, resultando em `Status = Cancelado`.

#### Scenario: Cancelamento de pagamento pendente
- **WHEN** o cancelamento é solicitado para um `Pagamento` com `Status = Pendente`
- **THEN** o `Status` do `Pagamento` passa a ser `Cancelado`

### Requirement: Cancelamento de Pagamento não pendente é rejeitado
O domínio SHALL rejeitar o cancelamento de um `Pagamento` cujo `Status` não seja `Pendente`, lançando uma exceção de domínio e mantendo o `Status` original inalterado.

#### Scenario: Tentativa de cancelar pagamento já cancelado
- **WHEN** o cancelamento é solicitado para um `Pagamento` com `Status = Cancelado`
- **THEN** uma exceção de domínio é lançada e o `Status` do `Pagamento` permanece `Cancelado`

### Requirement: Valor monetário é um Value Object imutável
O domínio SHALL representar o valor de um `Pagamento` como um Value Object imutável, comparável por valor (duas instâncias com o mesmo montante são consideradas iguais).

#### Scenario: Igualdade por valor
- **WHEN** dois valores monetários são criados com o mesmo montante
- **THEN** eles são considerados iguais entre si

### Requirement: Identificação única do Pagamento
O domínio SHALL atribuir a cada `Pagamento` criado um identificador único (`Id`) do tipo `Guid`, gerado no momento da criação, permitindo que camadas externas localizem o agregado individualmente.

#### Scenario: Id atribuído na criação
- **WHEN** um `Pagamento` é criado com sucesso
- **THEN** o `Pagamento` possui um `Id` do tipo `Guid` diferente de `Guid.Empty`

#### Scenario: Ids distintos para pagamentos distintos
- **WHEN** dois `Pagamento` são criados em chamadas separadas
- **THEN** os dois `Pagamento` possuem `Id` distintos entre si
