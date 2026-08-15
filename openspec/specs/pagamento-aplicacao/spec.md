# pagamento-aplicacao Specification

## Purpose

Orquestra o agregado `Pagamento` do Domain por meio de casos de uso (consultar, criar, cancelar) e define os ports que a Infrastructure deverá implementar para persistência e identificação do chamador, sem que a Application dependa de nenhum detalhe de infraestrutura.

## Requirements

### Requirement: Port de persistência de Pagamento
A Application SHALL definir um port de persistência que permite salvar um novo `Pagamento`, atualizar um `Pagamento` existente, buscar um `Pagamento` pelo seu `Id` e listar todos os `Pagamento` existentes, sem expor nenhum detalhe de armazenamento aos casos de uso. A atualização é uma operação explícita do port — uma transição de estado no agregado (ex.: cancelamento) só é considerada persistida depois de passar por ela, independentemente de qualquer implementação de repositório reter ou não a mesma referência de objeto em memória.

#### Scenario: Busca por Id inexistente
- **WHEN** um caso de uso solicita ao repositório um `Pagamento` por um `Id` que não existe
- **THEN** o repositório informa a ausência do registro sem lançar uma exceção de infraestrutura

### Requirement: Port de contexto do chamador
A Application SHALL definir um port `ICallerContext` que expõe a identidade do chamador autenticado (`Subject` e `ClientId`) para os casos de uso, sem que a Application dependa de `HttpContext` ou de qualquer detalhe de transporte HTTP.

#### Scenario: Caso de uso acessa a identidade do chamador
- **WHEN** um caso de uso precisa saber quem está executando a operação
- **THEN** ele obtém `Subject` e `ClientId` através do `ICallerContext`, sem acessar claims HTTP diretamente

### Requirement: Caso de uso Criar Pagamento
A Application SHALL permitir criar um novo `Pagamento` a partir de um valor monetário, delegando a validação da invariante de valor ao Domain e persistindo o resultado através do port de persistência.

#### Scenario: Criação com valor válido
- **WHEN** o caso de uso de criação é executado com um valor monetário positivo
- **THEN** um novo `Pagamento` com `Status = Pendente` é persistido e retornado com seu `Id`

#### Scenario: Criação com valor inválido
- **WHEN** o caso de uso de criação é executado com um valor monetário zero ou negativo
- **THEN** nenhum `Pagamento` é persistido e a exceção de domínio de valor inválido é propagada

### Requirement: Caso de uso Cancelar Pagamento
A Application SHALL permitir cancelar um `Pagamento` existente identificado pelo seu `Id`, delegando a regra de transição de estado ao Domain e persistindo o resultado através do port de persistência.

#### Scenario: Cancelamento de pagamento pendente existente
- **WHEN** o caso de uso de cancelamento é executado com o `Id` de um `Pagamento` com `Status = Pendente`
- **THEN** o `Pagamento` é persistido com `Status = Cancelado`

#### Scenario: Cancelamento de pagamento inexistente
- **WHEN** o caso de uso de cancelamento é executado com um `Id` que não corresponde a nenhum `Pagamento` persistido
- **THEN** nenhuma alteração é persistida e um erro de "não encontrado" é retornado ao chamador

#### Scenario: Cancelamento de pagamento não pendente
- **WHEN** o caso de uso de cancelamento é executado com o `Id` de um `Pagamento` cujo `Status` não é `Pendente`
- **THEN** nenhuma alteração é persistida e a exceção de domínio de cancelamento inválido é propagada

### Requirement: Caso de uso Consultar Pagamentos
A Application SHALL permitir consultar os `Pagamento` persistidos, retornando `Id`, valor monetário e `Status` de cada um através do port de persistência.

#### Scenario: Consulta com pagamentos existentes
- **WHEN** o caso de uso de consulta é executado e existem `Pagamento` persistidos
- **THEN** todos os `Pagamento` persistidos são retornados com `Id`, valor monetário e `Status`

#### Scenario: Consulta sem pagamentos existentes
- **WHEN** o caso de uso de consulta é executado e não existe nenhum `Pagamento` persistido
- **THEN** uma lista vazia é retornada

### Requirement: Casos de uso testáveis sem infraestrutura real
Os casos de uso da Application SHALL ser testáveis usando implementações fake/in-memory dos ports, sem depender de banco de dados, HTTP ou Keycloak reais.

#### Scenario: Execução de teste de caso de uso
- **WHEN** um teste de aplicação executa um caso de uso usando implementações fake/in-memory do port de persistência e do `ICallerContext`
- **THEN** o teste exercita a lógica do caso de uso sem qualquer chamada a infraestrutura real
