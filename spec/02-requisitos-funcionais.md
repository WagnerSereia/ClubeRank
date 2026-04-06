# Requisitos Funcionais - ClubeRank

## 1. Gestão de Organizações (Multi-tenant)

### RF1.1 - Criar Organização
**Descrição**: Sistema permite a criação de uma nova organização (clube, federação, empresa).

**Atores**: Usuário anônimo

**Fluxo Principal**:
1. Usuário acessa formulário de cadastro
2. Preenche: nome, email, telefone, modalidade esportiva
3. Define plano de assinatura
4. Recebe confirmação e acesso ao dashboard

**Critérios de Aceitação**:
- Dados obrigatórios validados
- Organização isolada em tenant separado
- Usuário criador é administrador padrão
- Ativação por email confirmatório

### RF1.2 - Configurar Organização
**Descrição**: Administrador da organização configura parâmetros básicos.

**Atores**: Administrador

**Campos Configuráveis**:
- Nome e logo
- Modalidade(s) esportiva(s)
- Parâmetros de ranking (fórmula pontuação, decaimento temporal)
- Horários de funcionamento
- Integração com calendário (opcional)

---

## 2. Gestão de Atletas

### RF2.1 - Cadastrar Atleta
**Descrição**: Criar novo registro de atleta com dados pessoais e de registro.

**Atores**: Administrador, Gestor de Atletas

**Dados Coletados**:
- Nome completo
- Gênero
- Email
- Telefone
- Filiação a categorias (A, B, C, Iniciante, Especial)

**Critérios de Aceitação**:
- Email único por organização
- Valor inicial de ranking definido (default: 1000 pontos)
- Histórico registrado: data criação, quem criou

### RF2.2 - Atualizar Dados de Atleta
**Descrição**: Modificar informações cadastrais de um atleta.

**Atores**: Administrador, Gestor de Atletas

**Restrições**:
- Data de nascimento não pode ser alterada após 30 dias
- Alterações registradas em auditoria
- Notificação ao atleta se email for alterado

### RF2.3 - Listar Atletas
**Descrição**: Visualizar lista completa de atletascom filtros e ordenação.

**Filtros Disponíveis**:
- Por categoria (A, B, C, Iniciante, Especial, por Sexo)
- Por status (ativo, inativo, suspenso)
- Por data de cadastro
- Por ranking (faixa de pontos)

**Ordenação**:
- Por nome, ranking, data cadastro, histórico de confrontos

**Paginação**: 50 atletas por página

### RF2.4 - Inativar / Reativar Atleta
**Descrição**: Suspender participação de atleta ou reativar.

**Atores**: Administrador

** Efeitos**:
- Atleta inativo não participa de novos sorteios
- Ranking congelado
- Histórico mantido
- Reativação restaura status anterior

### RF2.5 - Exportar Lista de Atletas
**Descrição**: Gerar arquivo CSV/Excel com dados de atletas.

**Incluir**:
- Nome, categoria, ranking, data cadastro, status

**Filtros**: Aplicáveis (por categoria, status, etc.)

---

## 3. Gestão de Rankings

### RF3.1 - Visualizar Ranking
**Descrição**: Exibir ranking em tempo real com posição, pontuação, tendência.

**Informações Exibidas**:
- Posição
- Nome do atleta
- Pontuação atual
- Variação da pontuação (últimos 7/30 dias)
- Trend visual (↑ ↓ →)
- Próximo confronto agendado

**Filtros**:
- Por categoria (A, B, C, Iniciante, Especial, por Sexo)
- Por período (atualizado hoje, esta semana, este mês)

**Ordenação**: Por pontuação (descendente)

### RF3.2 - Visualizar Histórico de Atletass
**Descrição**: Ver histórico completo de confrontos, resultados e variação de pontuação.

**Dados Exibidos**:
- Data do confronto
- Adversário
- Resultado (Ganhou/Perdeu/WO)
- Pontos antes/depois
- Variação (+18 pontos, -5 pontos)
- Notas/motivo da disputa

**Filtros**:
- Por período (últimos 7, 30, 90 dias, customizado)
- Por tipo de resultado

### RF3.3 - Recalcular Ranking
**Descrição**: Recalcular pontuação após evento excepcional (correção, desclassificação).

**Atores**: Administrador

**Processo**:
1. Selecionar evento que disparoutro
2. Confirmar alteração
3. Sistema recalcula ranking de atletas afetados
4. Gera relatório de mudanças
5. Notifica atletas impactados

**Auditoria**: Registra motivo, quem fez, timestamp

### RF3.4 - Aplicar Decaimento Temporal
**Descrição**: Reduzir pontuação de atletas inativos ou como regra competitiva.

**Parâmetros Configuráveis**:
- Frequência (semanal, mensal)
- Percentual de decaimento
- Atletas afetados (todos, inativos, específicos)

**Critérios de Aceitação**:
- Decaimento aplicado automaticamente conforme agendamento
- Registrado em auditoria com motivo
- Notificação opcional ao atleta

---

## 4. Gestão de Torneios

### RF4.1 - Criar Torneio
**Descrição**: Criar novo evento/torneio.

**Dados**:
- Nome
- Descrição
- Tipo (ladder, round-robin, série, outro)
- Categoria de atletas
- Data/período de realização
- Pontuação por vitória/derrota
- Status (planejado, em andamento, encerrado)

**Critérios de Aceitação**:
- Tourneio válido somente para categorias existentes
- Data início ≤ data fim
- Pontuação definida (default: Vitória +10, Derrota -5)

### RF4.2 - Gerenciar Fases do Torneio
**Descrição**: Criar e controlar fases de um torneio complexo (grupos, eliminatória).

**Tipos de Fases**:
- Grupo: Round-robin dentro de subgrupos
- Eliminatória: Mata-mata simples
- Qualificatória: Seleção de top N para próxima fase

**Ações**:
- Criar fase
- Designar atletas à fase
- Iniciar/encerrar fase
- Gerar próxima fase automaticamente baseado em resultados

### RF4.3 - Gerar Sorteios/Tabelas
**Descrição**: Criar confrontos para o torneio.

**Gatilhos**:
- Manual: Gestor clica botão "Gerar Confrontos"
- Automático: Scheduler em intervalos configuráveis

**Algoritmo**:
- Utiliza critérios definidos em Regras de Negócio
- Prioriza proximidade de ranking
- Evita repetições
- Respeita folgas configuradas

### RF4.4 - Visualizar Tabela do Torneio
**Descrição**: Exibir cronograma de confrontos e resultados.

**Visualizações**:
- Tabela de jogos: Atleta A vs B, Placar, Status, Data Registro
- Modo Olho de Peixe: Vários torneios na mesma tela
- Matriz de resultados (quem enfrentou quem)

**Filtros**: Por atleta, status (agendado, realizado, pendente)

### RF4.5 - Encerrar Torneio
**Descrição**: Finalizar um torneio e calcular resultados finais.

**Processo**:
1. Validar se todos confrontos foram preenchidos
2. Calcular ranking final do torneio
3. Gerar certificados/prêmios (se configurado)
4. Arquivar torneio
5. Notificar participantes

**Critérios de Aceitação**:
- Não é possível encerrar com confrontos pendentes (exceto com força)
- Relatório gerado com resultados

---

## 5. Registo de Confrontos e Resultados

### RF5.1 - Registrar Resultado de Confronto
**Descrição**: Informar resultado de um confronto realizado.

**Atores**: Gestor de Confrontos, Árbitro, ou o próprio Atleta (conforme permissão)

**Dados**:
- Resultado: Vitória (Atleta A) / Derrota / Empate / WO
- Placar (opcional, para documentação)
- Notas (motivoWO, anulação, etc.)

**Validações**:
- Ambos os atletas devem estar no mesmo torneio/categoria
- WO requer justificativa

**Efeitos Automáticos**:
- Ranking é atualizado imediatamente
- Confronto marcado como "realizado"
- Notificações enviadas
- Auditoria registrada

### RF5.2 - Editar Resultado de Confronto
**Descrição**: Modificar resultado já registrado (correção de erro).

**Restrições**:
- Timeouts: Máximo 24h após registro original
- Requer justificativa
- Apenas Administrador ou Gestor com permissão

**Efeitos**:
- Ranking recalculado para atletas afetados
- Histórico de alteração registrado
- Notificação aos atletas

### RF5.3 - Anular Confronto
**Descrição**: Invalidar um confronto anteriormente registrado.

**Motivos Permitidos**:
- Erro administrativo
- Violação de regras
- Circunstância atenuante

**Efeitos**:
- Ranking revertido para estado anterior
- Confronto removido/marcado como "anulado"
- Novo sorteio gerado se necessário

**Auditoria**: Registra motivo e quem cancelou

---

## 6. Notificações

### RF6.1 - Enviar Notificação de Confronto
**Descrição**: Notificar atletas sobre próximo confronto agendado.

**Canais**:
- Email
- Notificação in-app

**Conteúdo**:
- Adversário
- Link para confirmação de presença

**Timing**: Configurável (1 dia antes, etc.)

### RF6.2 - Configrurar Preferências de Notificação
**Descrição**: Permitir que usuários e athletes controlem quais notificações recebem.

**Opções**:
- Notificação de confronto agendado
- Notificação de resultado do confronto
- Notificação de alteração de ranking
- Digesto semanal/mensal
- Frequência dos envios

### RF6.3 - Notificar Sobre Alterações críticas
**Descrição**: Alertar sobre eventos importantes (rankingzaou alterado, confronto anulado, atleta promovido).

**Eventos**:
- Atleta entrou no top 10
- Atleta recebeu WO
- Ranking revertido por decisão administrativa
- Torneio finalizado

---

## 7. Dashboards e Relatórios

### RF7.1 - Dashboard de Gestor
**Descrição**: Visão geral da saúde do sistema com KPIs principais.

**Widgets**:
- Atletas cadastrados (total, novos esta semana)
- Confrontos processados (total, today, pending)
- Torneios ativos
- Últimas alterações de ranking (top 5 atletas com maior variação)
- Alertas (WO duplos, anomalias)

**Frescor**: Atualização em tempo real (ou máximo 1 min)

### RF7.2 - Dashboard de Atleta
**Descrição**: Visualização centrada no atleta.

**Informações**:
- Posição atual no ranking
- Pontuação e variação (semanal, mensal)
- Próximo confronto
- Histórico recente (últimos 5 confrontos)
- Tendência de pontuação (gráfico)
- Taxa de vitória

### RF7.3 - Gerar Relatório de Período
**Descrição**: Exportar relatório com estatísticas de um período.

**Métrica**:
- Número de confrontos realizados
- Estatística por atleta (vitórias, derrotas, WO)
- Ranking inicial vs final
- Maior variação positiva/negativa
- Formato: PDF ou Excel

---

## 8. Administração e Segurança

### RF8.1 - Gestão de Usuários (Administrador)
**Descrição**: Administrador cria usuários para a organização.

**Tipos de Usuários**:
- **Administrador**: Acesso total
- **Gestor de Atletas**: CRUD de atletas, visualização de ranking
- **Gestor de Confrontos**: Registra resultados
- **Auditor**: Visualização de logs e auditoria
- **Atleta**: Visualização limitada (próprio ranking, confrontos)

**Ações**:
- Criar/desativar usuário
- Atribuir permissões
- Resetar senha
- Editar informações de perfil

### RF8.2 - Auditoria de Operações
**Descrição**: Registrar todas as ações criticas para rastreabilidade.

**Eventos Auditados**:
- Alteração de ranking
- Cancelamento/anulção de confronto
- Modificação de regras
- Criação/deleção de usuário
- Acesso a relatórios sensíveis

**Informações Registradas**:
- Quem fez
- O quê foi feito
- Timestamp
- IP/sessão
- Antes/depois (dados alterados)

---

## 9. Integrações

### RF9.1 - Importar Atletas (CSV)
**Descrição**: Dados em lote de atletas via arquivo CSV.

**Formato Esperado**:
- Nome, Data Nascimento, Categoria, Ranking Inicial

**Validações**:
- Duplicatas detectadas (email)
- Campos obrigatórios preenchidos

**Resultado**: Relatório de sucesso/erro por linha

---

## Resumo de Requisitos por Prioridade

| Prioridade | RF# | Descrição |
|-----------|-----|-----------|
| **P0 - MVP** | RF2.1, RF3.1, RF4.1, RF5.1 | Atletas, Ranking, Torneio, Resultado |
|  | RF4.3, RF7.1 | Gerar Sorteios, Dashboard |
| **P1 - Essencial** | RF2.2, RF2.3, RF3.2, RF4.2 | Gerenciar atletas, histórico, fases |
|  | RF5.2, RF6, RF8.1 | Editar resultado, Notificações, Usuários |
| **P2 - Importante** | RF2.4, RF2.5, RF3.3, RF3.4 | Inativar, Exportar, Recalcular, Decaimento |
|  | RF4.4, RF7.2, RF7.3, RF8.2 | Visualizar tabela, Dashboards de atleta, Relatórios, Auditoria |
| **P3 - Nice-to-Have** | RF9.1, RF1.2 | Importar CSV, Configurações |