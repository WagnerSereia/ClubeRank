# Roadmap Técnico - ClubeRank

## Visão Geral

O roadmap técnico define a evolução do produto em fases claras, alinhadas com o escopo do MVP e a expansão planejada para escala e maturidade. Cada fase entrega um conjunto de funcionalidades e infraestrutura que permite crescimento seguro e controlado.

---

## Fase 0 - Preparação e Fundamentação
**Objetivo**: Estabelecer a base técnica e alinhar a arquitetura com a especificação.

**Entrega**:
- Repositório inicial com monorepo ou multi-repo definido
- Estrutura de pastas para Backend, Frontend e Infraestrutura
- Documentação inicial dos requisitos e arquitetura
- Ambiente local com Docker Compose e Docker Swarm single node
- Suporte a multi-tenant básico com TenantId

**Tarefas**:
- Validar todos os arquivos em `/spec`
- Definir modelo de domínio e modelo de dados
- Configurar DevOps inicial: CI pipeline, linting, testes básicos
- Criar boilerplate de backend em .NET 8
- Criar boilerplate de frontend em Angular 17
- Configurar ambiente Docker e Portainer

**Critérios de sucesso**:
- Projeto inicial pode ser levantado localmente
- Repositório tem README e documentação mínima
- Arquitetura está revisada e aprovada

---

## Fase 1 - MVP de Gestão de Ranking e Atletas
**Objetivo**: Entregar a base funcional do ClubeRank com cadastro de atletas, ranking e torneios simples.

**Entrega**:
- Cadastro e listagem de atletas
- Gestão de categorias e status
- Ranking simples configurável por organização
- Criação de torneios e associação de atletas
- Registro de resultados e atualização de ranking
- Dashboard básico de métricas
- Autenticação JWT e autorização por perfis

**Tarefas**:
- Implementar entidades de domínio e agregados
- Criar APIs REST para atletas, torneios e resultados
- Implementar regras de negócio de pontuação
- Construir telas de cadastro, listagem, e ranking em Angular
- Criar mecanismos de auditoria e logs
- Implementar cache para ranking e listagens

**Critérios de sucesso**:
- Usuário consegue usar o sistema end-to-end para gerenciar atletas e torneios
- Ranking é atualizado automaticamente após lançamento de resultados

---

## Fase 2 - Automação de Sorteios e Operações
**Objetivo**: Adicionar automação de sorteios e melhorar operações de torneio.

**Entrega**:
- Algoritmo de sorteio inteligente
- Regra de evitar repetição conforme histórico
- Visualização de confrontos agendados
- Notificações in-app e por email
- Relatórios e exportação de dados
- Gestão de resultados avançada (edição, anulação, recalculo)

**Tarefas**:
- Implementar `SorteioService` e critérios de pareamento
- Criar endpoints de geração de confrontos
- Implementar notificações e preferências
- Criar relatórios gerenciais e exportação CSV/Excel
- Suporte à edição de resultados e auditoria completa

**Critérios de sucesso**:
- Sistema gera confrontos automaticamente
- Operadores conseguem gerenciar torneios e resultados sem intervenção manual

---

## Fase 3 - Escala e Confiabilidade
**Objetivo**: Preparar a plataforma para uso real com maior volume e disponibilidade.

**Entrega**:
- Deploy confiável em Docker Swarm single node inicial
- Monitoramento de métricas e alertas
- Backup e recuperação de banco de dados
- Testes automatizados com cobertura relevante
- Políticas de segurança e compliance
- Otimizações de performance para 300 atletas/organização

**Tarefas**:
- Configurar Prometheus + Grafana para métricas
- Implementar backup scripts e políticas de retenção
- Criar suites de testes unitários e de integração
- Refinar autorizações e validações
- Otimizar consultas e caching

**Critérios de sucesso**:
- Sistema responde dentro dos SLAs definidos
- Deploy único é estável e monitorado
- Time de desenvolvimento tem pipeline CI/CD funcional

---

## Fase 4 - Evolução e Maturidade
**Objetivo**: Ampliar o produto com recursos avançados e preparar a eventual escalabilidade.

**Entrega**:
- Suporte a multi-node Swarm quando necessário
- Reestruturação de serviços para maior modularidade
- Relatórios avançados e dashboards ricos
- Políticas de governança e operação consolidada

**Tarefas**:
- Evoluir arquitetura para microservices quando cabível
- Implementar modularização do frontend e backend
- Adicionar suporte a métricas de negócio e UX
- Revisar custos e otimizar infraestrutura

**Critérios de sucesso**:
- Produto é sólido, escalável e pronto para crescimento
- Equipe tem processo de entrega contínua estabelecido
