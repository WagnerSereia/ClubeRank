# Arquitetura da Solução - ClubeRank

## Visão Geral da Arquitetura

**ClubeRank** adota uma arquitetura moderna baseada em **Domain-Driven Design (DDD)** com **Clean Architecture** e **Service Layer Pattern**, garantindo separação de responsabilidades, testabilidade e manutenibilidade.

---

## 1. Princípios Arquiteturais

### Arquitetura Hexagonal (Ports & Adapters)
- **Core Domain** independente de frameworks externos
- **Ports** definem interfaces para comunicação externa
- **Adapters** implementam ports para tecnologias específicas
- **Dependency Inversion** aplicada rigorosamente

### Domain-Driven Design (DDD)
- **Ubiquitous Language** consistente em todo o projeto
- **Bounded Contexts** para isolamento de domínios
- **Aggregates** como unidades de consistência
- **Domain Events** para comunicação assíncrona

### Clean Architecture
- **Camadas independentes** com responsabilidades claras
- **Dependências apontam inward** (para o domínio)
- **Testabilidade** em todas as camadas
- **Framework agnostic** no core

---

## 2. Estrutura de Camadas

```
📁 src/
├── 🏛️ Domain/                    # Camada de Domínio (Core)
│   ├── Entities/                 # Entidades de domínio
│   ├── ValueObjects/             # Objetos de valor
│   ├── Aggregates/               # Agregados
│   ├── Events/                   # Eventos de domínio
│   ├── Services/                 # Serviços de domínio
│   ├── Repositories/             # Interfaces de repositório
│   └── Specifications/           # Especificações
│
├── 🏢 Application/               # Camada de Aplicação
│   ├── UseCases/                 # Casos de uso
│   ├── Commands/                 # Commands (CQRS)
│   ├── Queries/                  # Queries (CQRS)
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Events/                   # Handlers de eventos
│   └── Services/                 # Serviços de aplicação
│
├── 🔧 Infrastructure/            # Camada de Infraestrutura
│   ├── Persistence/              # Repositórios concretos
│   │   ├── EFCore/              # Entity Framework
│   │   └── Migrations/          # Migrations DB
│   ├── External/                 # Integrações externas
│   ├── Messaging/                # Mensageria
│   ├── Caching/                  # Cache (Redis)
│   ├── Security/                 # Autenticação/Autorização
│   └── BackgroundJobs/           # Jobs em background
│
└── 🌐 API/                        # Camada de Apresentação
    ├── Controllers/              # Controllers REST
    ├── Middlewares/              # Middlewares
    ├── Filters/                  # Filtros
    ├── Models/                   # Request/Response models
    └── Swagger/                  # Documentação API
```

---

## 3. Camadas Detalhadas

### 3.1 Domain Layer (🏛️ Core)

**Responsabilidades**:
- Regras de negócio puras
- Modelagem do domínio
- Validações de negócio
- Lógica de domínio independente

**Conteúdo**:
- **Entities**: Atleta, Torneio, Organizacao, Usuario
- **Value Objects**: Email, Pontuacao, Categoria
- **Aggregates**: AtletaAggregate, TorneioAggregate
- **Domain Services**: RankingService, SorteioService
- **Domain Events**: ResultadoRegistrado, RankingAtualizado
- **Specifications**: AtletasAtivosSpec, ConfrontosPendentesSpec

**Regras**:
- Não depende de frameworks externos
- Não contém lógica de infraestrutura
- Testável unitariamente sem setup complexo

### 3.2 Application Layer (🏢 Casos de Uso)

**Responsabilidades**:
- Coordenação de operações complexas
- Orquestração entre aggregates
- Implementação de casos de uso
- CQRS para leitura/escrita separadas

**Padrões Aplicados**:
- **Command Query Responsibility Segregation (CQRS)**
- **Mediator Pattern** (MediatR)
- **Unit of Work** para transações

**Estrutura CQRS**:
```
Commands/                         # Write Operations
├── RegistrarResultadoCommand
├── CriarTorneioCommand
└── AtualizarRankingCommand

Queries/                          # Read Operations
├── ObterRankingQuery
├── ListarAtletasQuery
└── ObterDashboardQuery
```

### 3.3 Infrastructure Layer (🔧 Adaptadores)

**Responsabilidades**:
- Implementação de interfaces definidas no domínio
- Integração com sistemas externos
- Persistência de dados
- Comunicação externa

**Componentes**:
- **Repository Implementations**: EF Core, Dapper
- **External Services**: Email, SMS, Notificações
- **Caching**: Redis, In-memory
- **Messaging**: RabbitMQ, Azure Service Bus
- **Authentication**: JWT, OAuth2

### 3.4 API Layer (🌐 Apresentação)

**Responsabilidades**:
- Exposição de APIs REST
- Validação de entrada
- Serialização/Deserialização
- Tratamento de erros

**Tecnologias**:
- **ASP.NET Core** Web API
- **Swagger/OpenAPI** para documentação
- **FluentValidation** para validações
- **AutoMapper** para mapeamento DTO

---

## 4. Padrões de Design Aplicados

### 4.1 Domain-Driven Design Patterns

**Aggregate Pattern**:
```
AtletaAggregate (Root)
├── Atleta (Entity)
├── HistoricoConfrontos (Collection)
└── Estatisticas (Value Object)
```

**Repository Pattern**:
```csharp
public interface IAtletaRepository
{
    Task<Atleta> GetByIdAsync(AtletaId id);
    Task<IEnumerable<Atleta>> FindAsync(ISpecification<Atleta> spec);
    Task AddAsync(Atleta atleta);
    Task UpdateAsync(Atleta atleta);
}
```

**Specification Pattern**:
```csharp
public class AtletasAtivosSpec : Specification<Atleta>
{
    public AtletasAtivosSpec() =>
        Query.Where(a => a.Status == StatusAtleta.Ativo);
}
```

### 4.2 CQRS Pattern

**Commands**:
```csharp
public class RegistrarResultadoCommand : IRequest<Result>
{
    public AtletaId AtletaAId { get; set; }
    public AtletaId AtletaBId { get; set; }
    public ResultadoConfronto Resultado { get; set; }
    public string? Notas { get; set; }
}
```

**Queries**:
```csharp
public class ObterRankingQuery : IRequest<RankingDto>
{
    public OrganizacaoId OrganizacaoId { get; set; }
    public Categoria? Categoria { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 50;
}
```

### 4.3 Outros Padrões

**Factory Pattern** para criação de aggregates complexos:
```csharp
public class TorneioFactory
{
    public static Torneio CriarTorneioLadder(string nome, Organizacao org)
    {
        // Lógica de criação...
    }
}
```

**Observer Pattern** para domain events:
```csharp
public class RankingAtualizadoHandler : INotificationHandler<RankingAtualizadoEvent>
{
    public async Task Handle(RankingAtualizadoEvent notification)
    {
        // Enviar notificações, atualizar cache, etc.
    }
}
```

---

## 5. Tecnologias e Stack

### 5.1 Backend
- **Framework**: .NET 8.0 (ASP.NET Core)
- **Linguagem**: C# 12
- **ORM**: Entity Framework Core 8.0
- **Banco**: PostgreSQL (produção), SQL Server (dev)
- **Cache**: Redis
- **Mensageria**: RabbitMQ
- **Container**: Docker + Kubernetes

### 5.2 Frontend
- **Framework**: Angular 17 + TypeScript
- **State Management**: NgRx (Redux pattern for Angular)
- **UI Library**: Angular Material
- **Build Tool**: Angular CLI
- **PWA**: Angular PWA support

### 5.3 DevOps & Infraestrutura
- **CI/CD**: GitHub Actions / Azure DevOps
- **Container Registry**: Docker Hub / Azure Container Registry
- **Orchestration**: Docker Swarm (Linux cluster)
- **Management**: Portainer (visual container management)
- **Monitoring**: Prometheus + Grafana
- **Security**: Docker Secrets + TLS certificates

### 5.4 Ferramentas de Desenvolvimento
- **IDE**: Visual Studio 2022 / VS Code + Angular Language Service
- **Testing**: xUnit + NSubstitute + FluentAssertions (Backend), Jasmine + Karma (Frontend)
- **Code Quality**: SonarQube + ESLint (Angular) + Prettier
- **Documentation**: Swagger + MkDocs + Compodoc (Angular docs)
- **Container Tools**: Docker + Docker Compose + Portainer + Docker Swarm CLI

---

## 6. Estratégia de Deployment

### 6.1 Ambiente de Desenvolvimento
- **Local**: Docker Compose
- **Database**: PostgreSQL local
- **Cache**: Redis local
- **External APIs**: Mock services

### 6.2 Ambiente de Staging
- **Infraestrutura**: Single node Docker Swarm (Linux)
- **Database**: PostgreSQL em container
- **Cache**: Redis em container
- **Backup**: Automated daily via Docker volumes
- **Monitoring**: Portainer + basic health checks

### 6.3 Ambiente de Produção
- **Infraestrutura**: Single node Docker Swarm inicialmente (Linux), escalável para multi-node
- **Database**: PostgreSQL em container com backup
- **Cache**: Redis em container
- **Load Balancing**: nginx como reverse proxy
- **Backup**: Volume snapshots e automated scripts
- **Monitoring**: Portainer + Prometheus básico

### 6.4 Rolling Update Deployment
**Docker Swarm Strategy**:
```
🔵 Current Services (v1.2.3)
├── API service: 3 replicas
├── DB service: 1 replica
└── Cache service: 1 replica

🟢 New Services (v1.3.0)
├── API service: 3 replicas (rolling update)
├── DB migration: 1 replica (one-time)
└── Cache service: 1 replica (unchanged)
```

**Vantagens Swarm**:
- **Simplicidade**: Menos complexidade que Kubernetes
- **Visual Management**: Portainer para monitoramento visual
- **Linux Native**: Melhor performance em ambiente Linux
- **Custo**: Menor overhead operacional

---

## 7. Estratégia de Dados

### 7.1 Multi-tenancy
**Database per Tenant**:
- Cada organização tem seu próprio database
- Isolamento completo de dados
- Escalabilidade independente
- Backup isolado por tenant

**Vantagens**:
- Segurança máxima
- Performance previsível
- Compliance simplificado
- Migrações independentes

### 7.2 Estratégia de Cache
**Multi-level Caching**:
1. **Browser Cache**: Static assets (1 year)
2. **CDN Cache**: API responses (5 minutes)
3. **Application Cache**: Computed data (10 minutes)
4. **Database Cache**: Query results (1 hour)

### 7.3 Estratégia de Backup
**3-2-1 Rule**:
- **3 cópias** dos dados
- **2 tipos diferentes** de mídia
- **1 cópia off-site**

**Retention Policy**:
- Daily: 30 dias
- Weekly: 12 semanas
- Monthly: 12 meses
- Yearly: 7 anos

---

## 8. Segurança Arquitetural

### 8.1 Defense in Depth
**Camadas de Segurança**:
1. **Network**: Firewall Linux, Docker networks isolation, nginx reverse proxy
2. **Application**: Input validation, authentication, authorization
3. **Data**: Encryption at rest, access controls, auditing
4. **Infrastructure**: Docker secrets, container security scanning, Swarm security

### 8.2 Zero Trust Architecture
**Princípios**:
- **Never trust, always verify**
- **Least privilege access**
- **Micro-segmentation**
- **Continuous monitoring**

### 8.3 Compliance
**LGPD Compliance**:
- Data minimization
- Consent management
- Right to erasure
- Audit trails

---

## 9. Monitoramento e Observabilidade

### 9.1 Application Metrics
**KPIs Monitorados**:
- Response time por endpoint
- Error rate por serviço
- Throughput por organização
- Database connection pool usage
- Cache hit/miss ratio

### 9.2 Infrastructure Metrics
**System Health**:
- CPU/Memory usage
- Disk I/O
- Network latency
- Database performance
- Container health

### 9.3 Business Metrics
**Product KPIs**:
- Daily/Monthly Active Users
- Tournament completion rate
- User engagement (time spent)
- Feature adoption rates

### 9.4 Alerting Strategy
**Severity Levels**:
- **Critical**: System down, data loss
- **Warning**: Performance degradation
- **Info**: Unusual patterns

---

## 10. Estratégia de Testes

### 10.1 Test Pyramid
```
End-to-End Tests (10%)     ┌─────────┐
Integration Tests (20%)    │   🧪    │
Unit Tests (70%)          └─────────┘
```

### 10.2 Tipos de Testes

**Unit Tests**:
- Domain logic (entities, value objects, services)
- Application services
- Infrastructure adapters (mocks)

**Integration Tests**:
- Database operations
- External API calls
- Message queue interactions

**End-to-End Tests**:
- Critical user journeys
- API contracts
- UI interactions (Playwright)

### 10.3 Test Data Strategy
**Test Data Management**:
- **Factories** para criação de dados de teste
- **Fixtures** para cenários comuns
- **Builders** para objetos complexos
- **Seeders** para dados iniciais

---

## 11. Roadmap de Evolução Arquitetural

### Fase 1: MVP (Atual)
- Monolithic application
- Single database per tenant
- Basic caching
- **Single node Docker Swarm**
- Portainer para gerenciamento visual

### Fase 2: Scale (6 meses)
- Microservices decomposition (se necessário)
- Event-driven architecture
- Advanced caching strategies
- **Multi-node Swarm cluster** (adicionar nodes conforme crescimento)
- Load balancing distribuído

### Fase 3: Enterprise (12+ meses)
- Kubernetes migration (se escala demandar)
- Serverless functions
- AI/ML integration
- Global CDN optimization

---

## Diagrama Arquitetural Conceitual

```
┌─────────────────────────────────────────────────────────────┐
│                    🌐 Client Layer                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Browser/SPA  │  Mobile App  │  External APIs      │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │     🛡️ API Gateway      │
                    │  (Rate Limiting, Auth)  │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   🌐 API Layer          │
                    │  Controllers, DTOs     │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  🏢 Application Layer   │
                    │  Use Cases, CQRS       │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   🏛️ Domain Layer       │
                    │  Entities, Services    │
                    └────────────┬────────────┘
                                 │
               ┌────┴────┬─────────────────────┴────┬─────────┐
               │         │                          │         │
    ┌──────────▼────┐ ┌──▼─────────┐ ┌──────────────▼────┐ ┌──▼──────┐
    │ 🔧 Infra     │ │ 🔧 Infra   │ │     🔧 Infra       │ │ 🔧 Infra │
    │ Persistence  │ │ Messaging  │ │     External       │ │ Security │
    └──────────────┘ └────────────┘ └─────────────────────┘ └─────────┘
           │               │                   │                │
    ┌──────▼───────────────▼───────────────────▼────────────────▼─────┐
    │                    🗄️ External Systems                           │
    │  PostgreSQL  │  Redis  │  RabbitMQ  │  Email  │  Auth Provider │
    └──────────────────────────────────────────────────────────────────┘
```

**Orquestração**: Docker Swarm cluster com Portainer para gerenciamento visual
**Load Balancing**: nginx como edge router Swarm-aware
**Service Discovery**: Docker Swarm built-in DNS
**Secrets Management**: Docker Secrets para credenciais sensíveis

Esta arquitetura garante **manutenibilidade**, **escalabilidade** e **testabilidade**, seguindo as melhores práticas da indústria para aplicações modernas de SaaS.

## 12. Estratégia de Orquestração - Docker Swarm

### 12.1 Abordagem Single Node Inicial
**Justificativa**: Aplicação não crítica no MVP permite começar simples
- **Custos**: Single node reduz custos operacionais iniciais
- **Simplicidade**: Menos complexidade para setup e manutenção
- **Escalabilidade**: Fácil adicionar nodes conforme crescimento
- **Resiliência**: Aplicação stateless permite reconstrução rápida

**Quando Escalar**:
- Quando atingir ~200 atletas ativos
- Quando precisar de alta disponibilidade
- Quando carga de processamento exigir
- Quando backup distribuído for necessário

### 12.2 Configuração do Cluster
**Single Node Inicial**:
- **1 Node**: Manager + Worker (simplificado para MVP)
- **Load Balancer**: nginx como reverse proxy local
- **Escalabilidade**: Preparado para adicionar nodes quando necessário

**Services**:
- **API Service**: 2-3 replicas (para resiliência local)
- **Database Service**: 1 replica com volumes persistentes
- **Cache Service**: 1 replica
- **Background Jobs**: 1 replica

**Quando Escalar**:
- Adicionar worker nodes conforme crescimento
- Implementar manager nodes dedicados (3+ para HA)
- Configurar load balancing distribuído
- Implementar backup distribuído

### 12.3 Gerenciamento com Portainer
**Funcionalidades Utilizadas**:
- **Dashboard Visual**: Monitoramento em tempo real de containers
- **Stack Management**: Deploy de aplicações via docker-compose.yml
- **Service Scaling**: Ajuste automático/manual de réplicas
- **Logs Centralizados**: Visualização de logs de todos os serviços
- **Backup/Restore**: Gestão de volumes e configurações

### 12.4 Estratégia de Deploy
**Rolling Updates**:
- Atualização gradual dos serviços
- Zero-downtime deployments
- Rollback automático em caso de falha
- Health checks integrados

**Service Discovery**:
- DNS interno do Swarm
- Load balancing automático
- Service mesh básico via Swarm overlay networks