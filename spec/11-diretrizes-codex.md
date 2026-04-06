# Diretrizes Codex - ClubeRank

## Objetivo

Estabelecer um conjunto de práticas e padrões que garantam qualidade, consistência e manutenibilidade durante o desenvolvimento do ClubeRank.

---

## Princípios de Desenvolvimento

### 1. Domínio Primeiro
- Use **Domain-Driven Design (DDD)** como guia principal.
- Coloque regras de negócio no domínio, não nos controllers.
- Mantenha o domínio isolado de frameworks e camadas de infraestrutura.

### 2. Clean Architecture
- Separe em camadas: **Domain**, **Application**, **Infrastructure** e **API**.
- Dependências devem apontar para dentro (do externo para o domínio).
- Evite acoplamento entre controllers e lógica de negócio.

### 3. Multi-Tenant Seguro
- Respeite sempre o **TenantId / OrganizacaoId** em todas as queries.
- Implemente **query filters** no DbContext para isolar dados.
- Nunca exponha dados de outras organizações.

### 4. Segurança e Privacidade
- Trate dados pessoais com cuidado e restrinja coleta ao necessário.
- Não utilize CPF ou dados sensíveis desnecessários.
- Use autenticação JWT e roles para autorização.
- Logue eventos de segurança sem expor dados sensíveis.

### 5. Simplicidade e Transparência
- Prefira soluções simples e compreensíveis.
- Evite complexidade excessiva em regras de negócio.
- Documente decisões importantes no código e nos ADRs.

---

## Padrões de Código

### Backend (.NET)
- Use **C# 12** e padrão de estilo consistente.
- **DTOs** claros para API e mapeamento com AutoMapper ou similar.
- **Repositório** somente para persistência; regras ficam no domínio.
- **Services de Aplicação** orquestram use cases e mixam dependências.
- **Domain Events** para comunicação entre componentes do domínio.

### Frontend (Angular)
- Utilize **Angular Style Guide** oficial.
- Componentes pequenos e reutilizáveis.
- **NgRx** para gerenciamento de estado global.
- **Angular Material** para consistência visual.
- Separe lógica de apresentação e acesso a dados.

### Qualidade de Código
- Mantenha **cobertura de testes** significativa (> 80% no backend inicialmente).
- Evite **duplicação** de lógica.
- Use **naming conventions** consistentes e legíveis.
- Aplique **SOLID** e **Clean Code**.

---

## Diretrizes de Projeto

### Controllers e Use Cases
- Controllers devem ser finos: receber request, chamar caso de uso e retornar response.
- A lógica principal pertence aos **casos de uso** ou aos **serviços de domínio**.
- Validações de negócio complexas não ficam nos controllers.

### Regras de Negócio
- Codifique regras em **Entidades**, **Serviços de Domínio** ou **Specifications**.
- Use **Value Objects** para encapsular invariantes.
- Todos os critérios de negócio devem ser rastreáveis aos documentos de `/spec`.

### Persistência
- Prefira **EF Core** com `DbContext` e `Query Filters` para multi-tenancy.
- Use **Owned Types** para objetos de valor como Email e Telefone.
- Garanta índices em colunas de pesquisa frequente e chaves estrangeiras.

### Configuração
- Use configuração externalizada (variáveis de ambiente).
- Dados sensíveis nunca em código-fonte.
- Separar configurações por ambiente: dev, staging, prod.

---

## Práticas de Desenvolvimento

### Commits e Branching
- Branches por feature/bugfix/hotfix.
- Commits curtos, descritivos e em inglês simples.
- Use pull requests com revisão obrigatória.

### Testes
- Escreva **unit tests** para domínio e serviços.
- Escreva **integration tests** para persistência e APIs.
- Inclua ao menos uma cobertura de cenário end-to-end crítico.

### Documentação
- Atualize README e documentação sempre que houver mudança arquitetural.
- Mantenha `/spec` como fonte de verdade.
- Documente decisões arquiteturais em ADRs.

### Observabilidade
- Use logs estruturados e centralizados.
- Monitore métricas de performance e erros.
- Faça tracing de requests críticos.

---

## Padrões Específicos do Projeto

- **Não** usar Saga pattern nesta fase.
- **Sim** usar `nginx` como proxy reverso no Docker Swarm.
- **Sim** utilizar Docker Swarm single node para MVP.
- **Não** coletar CPF ou dados sensíveis desnecessários.
- **Sim** suportar categorias fixas: A, B, C, Iniciante, Especial e por Sexo.
- **Sim** priorizar simplicidade: jogos são lançados sem controle de data/hora.

---

## Critérios de Aceitação para Código

- Código deve estar alinhado com `/spec`.
- Todo novo serviço deve ter teste unitário associado.
- Todas as APIs devem ser documentadas no Swagger.
- Nenhuma regra de negócio deve estar em controller.
- Todo acesso a dados deve respeitar `OrganizacaoId`.
