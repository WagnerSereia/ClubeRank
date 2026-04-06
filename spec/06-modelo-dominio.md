# Modelo de Domínio - ClubeRank

## Visão Geral do Domínio

**ClubeRank** gerencia competições esportivas através de um sistema de ranking automatizado, sorteios inteligentes e gestão de torneios. O domínio central gira em torno da **competição justa** entre atletas, com ênfase em **transparência** e **automação**.

---

## 1. Contextos Delimitados (Bounded Contexts)

### 1.1 Core Domain: Gestão de Competições
**Responsabilidades**:
- Gestão de atletas e seus rankings
- Algoritmos de sorteio e pareamento
- Registro e validação de resultados
- Cálculo de pontuações

### 1.2 Supporting Domain: Administração
**Responsabilidades**:
- Gestão de organizações e usuários
- Configurações do sistema
- Auditoria e compliance
- Relatórios e analytics

### 1.3 Generic Domain: Infraestrutura
**Responsabilidades**:
- Autenticação e autorização
- Notificações e comunicações
- Persistência e cache
- Integrações externas

---

## 2. Entidades de Domínio (Entities)

### 2.1 Atleta (Athlete)
**Identidade**: AtletaId (GUID)

**Atributos**:
- **Nome**: NomeCompleto (Value Object)
- **Gênero**: Genero (Masculino, Feminino, Outro)
- **Email**: Email (Value Object)
- **Telefone**: Telefone? (Value Object, opcional)
- **DataCadastro**: DateTime
- **Status**: StatusAtleta (Ativo, Inativo, Suspenso)
- **Categoria**: Categoria (A, B, C, Iniciante, Especial)
- **OrganizacaoId**: OrganizacaoId (referência)

**Comportamentos**:
- AlterarDadosPessoais()
- Ativar()
- Inativar()
- Suspender()
- AlterarCategoria()

**Regras de Negócio**:
- Email deve ser único por organização
- Data de nascimento não pode ser alterada após 30 dias
- Apenas administradores podem alterar categoria

### 2.2 Organização (Organization)
**Identidade**: OrganizacaoId (GUID)

**Atributos**:
- **Nome**: string
- **Email**: Email (Value Object)
- **Telefone**: Telefone (Value Object)
- **Modalidade**: string
- **Logo**: UrlImagem? (Value Object, opcional)
- **Configuracoes**: ConfiguracaoOrganizacao (Value Object)
- **Status**: StatusOrganizacao (Ativa, Suspensa, Inativa)
- **DataCadastro**: DateTime
- **Plano**: TipoPlano (Basico, Profissional, Empresarial)

**Comportamentos**:
- AtualizarConfiguracoes()
- AlterarPlano()
- Suspender()
- Reativar()

**Regras de Negócio**:
- Máximo 300 atletas no plano básico
- Configurações afetam todos os cálculos de ranking

### 2.3 Torneio (Tournament)
**Identidade**: TorneioId (GUID)

**Atributos**:
- **Nome**: string
- **Descricao**: string?
- **Tipo**: TipoTorneio (Ladder, RoundRobin, Eliminatoria, Serie)
- **Status**: StatusTorneio (Planejado, Ativo, Encerrado, Cancelado)
- **DataInicio**: DateTime?
- **DataFim**: DateTime?
- **OrganizacaoId**: OrganizacaoId
- **Categoria**: Categoria?
- **Configuracao**: ConfiguracaoTorneio (Value Object)

**Comportamentos**:
- Iniciar()
- Encerrar()
- Cancelar()
- AdicionarAtleta()
- RemoverAtleta()

**Regras de Negócio**:
- Não pode iniciar sem pelo menos 2 atletas
- Data fim deve ser posterior à data início
- Apenas atletas da mesma categoria (se especificada)

### 2.4 Confronto (Match)
**Identidade**: ConfrontoId (GUID)

**Atributos**:
- **AtletaAId**: AtletaId
- **AtletaBId**: AtletaId
- **TorneioId**: TorneioId
- **Status**: StatusConfronto (Agendado, Realizado, Cancelado)
- **Resultado**: ResultadoConfronto? (Value Object)
- **DataRegistro**: DateTime?
- **Notas**: string?

**Comportamentos**:
- RegistrarResultado()
- Cancelar()
- Reagendar()

**Regras de Negócio**:
- Ambos atletas devem estar ativos
- Resultado só pode ser registrado uma vez
- WO requer justificativa obrigatória

### 2.5 Usuario (User)
**Identidade**: UsuarioId (GUID)

**Atributos**:
- **Nome**: string
- **Email**: Email (Value Object)
- **Senha**: Senha (Value Object, criptografada)
- **Perfil**: PerfilUsuario (Admin, GestorAtletas, GestorConfrontos, Atleta, Auditor)
- **OrganizacaoId**: OrganizacaoId?
- **Status**: StatusUsuario (Ativo, Inativo, Bloqueado)
- **UltimoAcesso**: DateTime?

**Comportamentos**:
- AlterarSenha()
- AlterarPerfil()
- Bloquear()
- Desbloquear()

**Regras de Negócio**:
- Email deve ser único globalmente
- Senha deve ter complexidade mínima
- Perfis têm permissões específicas

---

## 3. Value Objects

### 3.1 NomeCompleto
```csharp
public class NomeCompleto : ValueObject
{
    public string PrimeiroNome { get; }
    public string Sobrenome { get; }

    public NomeCompleto(string primeiroNome, string sobrenome)
    {
        // Validações...
    }

    public string NomeFormatado => $"{PrimeiroNome} {Sobrenome}";
}
```

### 3.2 Email
```csharp
public class Email : ValueObject
{
    public string Valor { get; }

    public Email(string email)
    {
        // Regex validation...
    }
}
```

### 3.3 Pontuacao (Score)
```csharp
public class Pontuacao : ValueObject
{
    public int Valor { get; }
    public DateTime DataAtualizacao { get; }

    public Pontuacao(int valor)
    {
        // Validações: 0 <= valor <= 5000
    }

    public Pontuacao Adicionar(int pontos) =>
        new Pontuacao(Valor + pontos);
}
```

### 3.4 ResultadoConfronto
```csharp
public class ResultadoConfronto : ValueObject
{
    public Vencedor Vencedor { get; }
    public TipoResultado Tipo { get; } // Vitoria, Derrota, Empate, WO
    public string? Placar { get; }
    public DateTime DataRealizacao { get; }

    public enum Vencedor { AtletaA, AtletaB, Empate }
    public enum TipoResultado { Normal, Walkover }
}
```

### 3.5 ConfiguracaoOrganizacao
```csharp
public class ConfiguracaoOrganizacao : ValueObject
{
    public Pontuacao Vitoria { get; }
    public Pontuacao Derrota { get; }
    public Pontuacao DerrotaWO { get; }
    public int PontuacaoInicial { get; }
    public int DiasDecaimento { get; }
    public int PontosDecaimento { get; }
}
```

---

## 4. Agregados (Aggregates)

### 4.1 AtletaAggregate
**Root Entity**: Atleta

**Entidades Internas**:
- HistoricoConfrontos (Collection)
- EstatisticasAtleta (Value Object)

**Value Objects**:
- PontuacaoAtual
- ListaConfrontosRecentes

**Regras de Consistência**:
- Pontuação sempre atualizada após resultado
- Histórico imutável (apenas adição)
- Estatísticas calculadas automaticamente

**Domain Events**:
- AtletaCadastrado
- PontuacaoAtualizada
- CategoriaAlterada

### 4.2 TorneioAggregate
**Root Entity**: Torneio

**Entidades Internas**:
- ListaAtletas (Collection)
- ListaConfrontos (Collection)
- ConfiguracaoTorneio

**Regras de Consistência**:
- Confrontos só entre atletas do torneio
- Status do torneio controla operações permitidas
- Resultados afetam ranking global

**Domain Events**:
- TorneioCriado
- TorneioIniciado
- TorneioEncerrado
- ConfrontoAgendado

### 4.3 OrganizacaoAggregate
**Root Entity**: Organização

**Entidades Internas**:
- ListaUsuarios (Collection)
- ConfiguracaoOrganizacao
- EstatisticasOrganizacao

**Regras de Consistência**:
- Configurações aplicam a todos os atletas
- Limites de plano respeitados
- Usuários só da organização

**Domain Events**:
- OrganizacaoCriada
- ConfiguracaoAlterada
- LimitePlanoAtingido

---

## 5. Serviços de Domínio (Domain Services)

### 5.1 RankingService
**Responsabilidades**:
- Calcular pontuação baseada em resultado
- Aplicar decaimento temporal
- Recalcular ranking após alterações

**Métodos**:
```csharp
public Pontuacao CalcularNovaPontuacao(
    Pontuacao atual,
    ResultadoConfronto resultado,
    ConfiguracaoOrganizacao config)

public IEnumerable<RankingItem> CalcularRanking(
    IEnumerable<Atleta> atletas,
    Categoria? categoria)
```

### 5.2 SorteioService
**Responsabilidades**:
- Algoritmo de pareamento inteligente
- Evitar repetições quando possível
- Otimizar diversidade de confrontos

**Métodos**:
```csharp
public IEnumerable<ParAtletas> GerarSorteio(
    IEnumerable<Atleta> atletas,
    IEnumerable<Confronto> historico)

public bool ValidarPareamento(
    Atleta atletaA,
    Atleta atletaB,
    IEnumerable<Confronto> historico)
```

### 5.3 ValidacaoService
**Responsabilidades**:
- Validar regras de negócio complexas
- Verificar consistência de agregados
- Aplicar políticas de segurança

**Métodos**:
```csharp
public ValidationResult ValidarRegistroResultado(
    Confronto confronto,
    ResultadoConfronto resultado)

public ValidationResult ValidarCriacaoTorneio(
    Torneio torneio,
    Organizacao organizacao)
```

---

## 6. Eventos de Domínio (Domain Events)

### 6.1 Eventos de Atleta
- **AtletaCadastrado**: Novo atleta registrado
- **PontuacaoAtualizada**: Ranking alterado após resultado
- **CategoriaAlterada**: Atleta mudou de categoria
- **AtletaSuspenso**: Atleta temporariamente inativo

### 6.2 Eventos de Torneio
- **TorneioCriado**: Novo torneio configurado
- **TorneioIniciado**: Competição começou
- **ConfrontoAgendado**: Novo pareamento criado
- **ResultadoRegistrado**: Confronto finalizado
- **TorneioEncerrado**: Competição finalizada

### 6.3 Eventos de Sistema
- **RankingRecalculado**: Ranking global atualizado
- **SorteioExecutado**: Novos confrontos gerados
- **LimiteOrganizacaoAtingido**: Organização próxima do limite
- **AuditoriaRegistrada**: Ação crítica auditada

---

## 7. Repositórios (Repositories)

### 7.1 Interfaces de Domínio
```csharp
public interface IAtletaRepository
{
    Task<Atleta> GetByIdAsync(AtletaId id);
    Task<IEnumerable<Atleta>> GetByOrganizacaoAsync(OrganizacaoId orgId);
    Task<IEnumerable<Atleta>> FindAsync(ISpecification<Atleta> spec);
    Task AddAsync(Atleta atleta);
    Task UpdateAsync(Atleta atleta);
}

public interface ITorneioRepository
{
    Task<Torneio> GetByIdAsync(TorneioId id);
    Task<IEnumerable<Torneio>> GetAtivosByOrganizacaoAsync(OrganizacaoId orgId);
    Task AddAsync(Torneio torneio);
    Task UpdateAsync(Torneio torneio);
}

public interface IConfrontoRepository
{
    Task<Confronto> GetByIdAsync(ConfrontoId id);
    Task<IEnumerable<Confronto>> GetPendentesByAtletaAsync(AtletaId atletaId);
    Task<IEnumerable<Confronto>> GetHistoricoAsync(AtletaId atletaAId, AtletaId atletaBId);
    Task AddAsync(Confronto confronto);
    Task UpdateAsync(Confronto confronto);
}
```

### 7.2 Especificações (Specifications)
```csharp
public class AtletasAtivosSpec : Specification<Atleta>
{
    public AtletasAtivosSpec(OrganizacaoId orgId)
    {
        Query.Where(a => a.OrganizacaoId == orgId && a.Status == StatusAtleta.Ativo);
    }
}

public class ConfrontosPendentesSpec : Specification<Confronto>
{
    public ConfrontosPendentesSpec(AtletaId atletaId)
    {
        Query.Where(c => (c.AtletaAId == atletaId || c.AtletaBId == atletaId)
                      && c.Status == StatusConfronto.Agendado);
    }
}
```

---

## 8. Relacionamentos e Dependências

### 8.1 Diagrama de Relacionamentos
```
🏢 Organização
├── 👥 Usuários (1:N)
├── 🏃 Atletas (1:N)
├── 🏆 Torneios (1:N)
└── ⚙️ Configurações (1:1)

🏃 Atleta
├── 🏢 Organização (N:1)
├── 🏆 Torneios (N:M)
├── ⚔️ Confrontos (N:M)
└── 📊 Ranking (1:1)

🏆 Torneio
├── 🏢 Organização (N:1)
├── 🏃 Atletas (N:M)
└── ⚔️ Confrontos (1:N)

⚔️ Confronto
├── 🏃 Atleta A (N:1)
├── 🏃 Atleta B (N:1)
├── 🏆 Torneio (N:1)
└── 📊 Resultado (1:1)
```

### 8.2 Regras de Integridade
- **Organização**: Container isolado (multi-tenant)
- **Atleta**: Pertence a uma organização
- **Torneio**: Escopo organizacional
- **Confronto**: Vinculado a torneio e atletas
- **Usuário**: Pode ser global (super admin) ou organizacional

---

## 9. Casos de Uso Principais

### 9.1 Registrar Resultado de Confronto
**Ator**: Gestor de Confrontos

**Fluxo Principal**:
1. Sistema valida confronto pendente
2. Registra resultado e timestamp
3. Calcula nova pontuação via RankingService
4. Atualiza ranking do atleta
5. Publica evento RankingAtualizado
6. Envia notificações

**Regras Aplicadas**:
- RN3.2: Validação de resultado
- RN1.1: Cálculo de pontuação
- RN5.1: Auditoria obrigatória

### 9.2 Executar Sorteio de Confrontos
**Ator**: Sistema (agendado)

**Fluxo Principal**:
1. Filtra atletas elegíveis
2. Agrupa por categoria/rating
3. Aplica algoritmo de sorteio
4. Valida constraints (repetição, diversidade)
5. Cria confrontos agendados
6. Notifica atletas

**Regras Aplicadas**:
- RN2.1: Proximidade de ranking
- RN2.2: Evitar repetição quando possível
- RN2.3: Maximizar diversidade

### 9.3 Criar Novo Torneio
**Ator**: Administrador

**Fluxo Principal**:
1. Valida permissões e limites
2. Cria torneio com configuração
3. Associa atletas elegíveis
4. Define regras específicas
5. Publica evento TorneioCriado

**Regras Aplicadas**:
- RN4.1: Configuração de pontos
- Limites organizacionais
- Validação de categoria

---

## 10. Invariantes de Domínio

### 10.1 Invariantes de Atleta
- Pontuação sempre ≥ 0 e ≤ 5000
- Status ativo requer organização ativa
- Categoria deve ser compatível com organização
- Email único por organização

### 10.2 Invariantes de Torneio
- Data fim > data início (se definidas)
- Mínimo 2 atletas para iniciar
- Todos confrontos devem ser válidos
- Status transitions seguem fluxo definido

### 10.3 Invariantes de Confronto
- Ambos atletas devem existir e estar ativos
- Resultado só pode ser definido uma vez
- Vencedor deve ser um dos atletas (exceto empate)
- WO requer justificativa

### 10.4 Invariantes Globais
- Organização não pode exceder limites do plano
- Rankings sempre consistentes após alterações
- Auditoria completa de operações críticas
- Integridade referencial mantida

---

## 11. Linguagem Ubíqua (Ubiquitous Language)

**Termos Essenciais**:
- **Atleta**: Participante da competição
- **Ranking**: Classificação por pontuação
- **Sorteio**: Processo de pareamento automático
- **Confronto**: Encontro entre dois atletas
- **WO (Walkover)**: Vitória por ausência do adversário
- **Categoria**: Agrupamento por nível (A, B, C, etc.)
- **Organização**: Clube/federação isolada
- **Torneio**: Evento competitivo estruturado

**Padrões de Nomenclatura**:
- Entidades: Substantivos (Atleta, Torneio)
- Value Objects: Substantivos compostos (NomeCompleto, Pontuacao)
- Serviços: Verbos + Substantivos (RankingService, SorteioService)
- Eventos: Passado + Substantivo (ResultadoRegistrado, RankingAtualizado)

---

Este modelo de domínio captura a essência do negócio de gestão de competições esportivas, garantindo que o software reflita fielmente as regras e processos do mundo real.