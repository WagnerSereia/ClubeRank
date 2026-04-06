# Regras de Negócio - ClubeRank

## 1. Regras de Ranking

## 1. Regras de Ranking

### RN1.1 - Pontuação Simples e Configurável
**Sistema**: Pontuação direta configurável por clube

**Parâmetros Configuráveis** (por organização):
- **Vitória**: +X pontos (ex: +10)
- **Derrota**: +Y pontos (ex: -5)
- **Derrota por WO**: +Z pontos de penalidade (ex: -20)

**Fórmula**:
```
Novo Rating = Rating Anterior + Pontos do Resultado
```

**Exemplos**:
- Vitória: 1000 + 10 = **1010 pontos**
- Derrota: 1000 - 5 = **995 pontos**
- Derrota por WO: 1000 - 20 = **980 pontos**

**Configuração Padrão**:
- Vitória: +10 pontos
- Derrota: -5 pontos
- Derrota por WO: -20 pontos
- Rating Inicial: 1000 pontos

**Vantagens**:
- Simples de entender
- Fácil de configurar por clube
- Transparente para atletas
- Flexível para diferentes modalidades

### RN1.3 - Variação Mínima de Pontuação
**Regra**: Nenhuma alteração de ranking < ±1 ponto.

**Efeito**: Arredonda para 1 ponto mínimo de variação

**Justificativa**: Evita alterações imperceptíveis no ranking

### RN1.4 - Limite de Pontuação Máxima
**Teto**: 5000 pontos (limite máximo alcançável)
**Piso**: 0 pontos (mínimo, nunca negativo)

**Justificativa**: Mantém escala controlada,evita valores absurdos

### RN1.5 - Decaimento Temporal (Inatividade)
**Condição**: Atleta sem confrontos por 30+ dias

**Decaimento**: -10 pontos a cada 7 dias após o 30° dia

**Limite**: Não pode decair abaixo de 900 pontos (mínimo garantido para ativo)

**Exceção**: Pode ser desativado por configuração organizacional

**Máximo Decaimento**: -100 pontos/mês por inatividade

---

## 2. Regras de Sorteio de Confrontos

### RN2.1 - Critério Primário: Proximidade de Ranking
**Objetivo**: Parear atletas com ratings próximos para competições equilibradas.

**Algoritmo**:
1. Ordenar todos os atletas por rating (descendente)
2. Para cada atleta, calcular "alvos ideais" (range ±50-150 pontos)
3. Priorizar pareamento com alteta mais prócimo disponível

**Fórmula de Proximidade**:
```
Distância = |Rating Jogador A - Rating Jogador B|
Proximidade Score = 1000 - Distância  (maior é melhor)
```

**Exemplo**:
- Jogador X: 1000 pontos
- Candidatos:
  - Y (1050 pontos): Proximidade = 1000 - 50 = **950**
  - Z (1180 pontos): Proximidade = 1000 - 180 = **820**
- **Y é a melhor escolha**

### RN2.2 - Critério Secundário: Evitar Repetição (Quando Possível)
**Objetivo**: Preferir parear atletas que ainda não enfrentaram, mas permitir repetição quando não há alternativas.

**Lógica Preferencial**:
1. **Primeira prioridade**: Parear com atletas nunca enfrentados
2. **Segunda prioridade**: Parear com atletas enfrentados há mais tempo
3. **Último recurso**: Permitir repetição apenas se não há outras opções viáveis

**Penalidade de Repetição** (aplicada quando necessário):
```
Score Final = Proximidade Score - (Número Enfrentamentos × 100) - (Dias Desde Último Enfrento × 2)
```

**Condições para Permitir Repetição**:
- Não há atletas elegíveis nunca enfrentados disponíveis
- O atleta já enfrentou todos os possíveis oponentes viáveis
- Número de enfrentamentos ≤ 3 vezes nos últimos 90 dias

**Exemplo**:
- Jogadores A e B: Proximidade Score 900
- A e B se enfrentaram 1x há 30 dias: Score = 900 - 100 - (30×2) = **760**
- Se A já enfrentou todos os outros possíveis: **PERMITIDO** (mas com baixa prioridade)

### RN2.3 - Critério Terciário: Distribuição de Confrontos
**Objetivo**: Maximizar diversidade de oponentes, mas permitir repetição quando necessário.

**Métrica**: Índice de Diversidade = (Número de Oponentes Únicos / Número Total de Confrontos)

**Alvo Ideal**: Índice ≥ 0.8 (pelo menos 80% de oponentes diferentes)

**Lógica Adaptativa**:
- Se diversidade alta disponível: Priorizar "novo oponente"
- Se diversidade limitada: Permitir repetição com oponente enfrentado há mais tempo
- Nunca forçar repetição se há alternativas viáveis

**Bonus de Diversidade**:
```
Se Índice Atual < 0.8: +50 pontos para pareamento com novo oponente
Se Índice Atual ≥ 0.8: +10 pontos para qualquer pareamento válido
```

### RN2.4 - Algoritmo Completo de Sorteio

**Entrada**: Lista de atletas aptos, período (ex: próxima semana)

**Processo**:
```
1. Filtrar atletas elegíveis:
   - Status = ativo
   - Sem torneio bloqueado em período
   - Sem folga agendada

2. Ordenar por rating (desc) e aplicar K-means clustering:
   - Grupos de 4-6 atletas com ratings similares
   
3. Para cada cluster:
   - **Fase 1**: Tentar pareamento apenas com atletas nunca enfrentados
   - **Fase 2**: Se necessário, incluir atletas enfrentados há mais tempo
   - **Fase 3**: Último recurso, permitir repetição com penalidade
   - Calcular Score para cada combinação:
     Score = Proximidade + Penalidade Repetição + Bonus Diversidade
   
4. Otimização de Alocação:
   - Usar algoritmo Hungaro ou similar para máx Score total
   - Priorizar diversidade, depois proximidade, depois evitar repetição
   - Garantir que cada atleta ≤ N confrontos por semana
   
5. Validar resultado final:
   - Máximo 3 repetições nos últimos 90 dias por par
   - Distribuição balanceada quando possível
   - Todos os atletas participam proporcionalmente

6. Retornar: Lista de pares alocados com reasoning
```

**Constraints Obrigatórios**:
- Cada atleta máximo 1 confronto/resultado por dia
- Cada atleta máximo 7 confrontos/semana (configurável)
- Sem confronto agendado se já existe em pendência para a semana

---

## 3. Regras de Resultado e Validação

### RN3.1 - Tipos de Resultado Permitidos
**Válidos Para Ladder**:
- Vitória (Ganhou)
- Derrota (Perdeu)
- WO - Walkover (ausência/desistência)

**Válidos Para Torneio Eliminatório**:
- Vitória
- Derrota
- Empate (se torneio permitir, senão vai para prorrogação/penalidades)
- Suspensão/Protesto (adiantar resultado final)

### RN3.2 - Validação de Resultado
**Regra**:
- Resultado pode ser registrado a qualquer momento
- Foco na integridade dos dados, não no timing

**Exceção**: Administrative override (por administrador com motivo registrado)

### RN3.3 - Tratamento de WO (Walkover)

**Causas Permitidas**:
- Jugador não compareceu
- Desistência voluntária
- Desclassificação por violação de regras
- Motivo de força maior (acidente, morte na família, etc)

### RN3.3 - Tratamento de WO (Walkover)

**Causas Permitidas**:
- Jogador não compareceu
- Desistência voluntária
- Desclassificação por violação de regras
- Motivo de força maior (acidente, morte na família, etc)

**Pontuação de WO**:
- **Vencedor (por WO)**: Recebe os pontos de vitória configurados
  - Exemplo: Se vitória = +10 pontos, ganha +10 por WO
- **Perdedor (WO)**: Recebe os pontos de derrota por WO configurados
  - Exemplo: Se derrota por WO = -20 pontos, perde -20 pontos

**Nota**: Os pontos são os mesmos configurados na organização, mantendo consistência e simplicidade.

**Aviso de WO Duplo**:
- Se um atleta recebe 2 WO em 14 dias: **ALERTA CRÍTICO**
- Se 3 WO em 30 dias: **SUSPENSÃO AUTOMÁTICA de 7 dias**
- (Regra configurável por organização)

**Aviso de WO Duplo**:
- Se um atleta recebe 2 WO em 14 dias: **ALERTA CRÍTICO**
- Se 3 WO em 30 dias: **SUSPENSÃO AUTOMÁTICA de 7 dias**
- (Regra configurável por organização)

### RN3.4 - Desempate em Ranking
**Quando Dois Atletas com Mesma Pontuação**:

**Tiebreakers (ordem):**
1. Maior número de vitórias diretas (confronto direto)
2. Maior variação positiva (confronos ganhos - perdidos)
3. Melhor taxa de vitória (vitórias / total confrontos)
4. Menor número de WO
5. Data cadastre mais antiga (Tie-break técnico)

**Exemplo**:
- Atleta X: 1200 pts | 8 V - 2 D | Taxa 80% | 1 WO
- Atleta Y: 1200 pts | 7 V - 2 D | Taxa 77% | 0 WO
- **Y fica acima** (menos WO - item 4)

---

## 4. Regras de Torneios

### RN4.1 - Configuração de Pontos por Vitória/Derrota
**Sistema**: Usa as mesmas regras de pontuação do ranking da organização

**Pontos Aplicados**:
- Vitória: +X pontos (configurado na organização)
- Derrota: +Y pontos (configurado na organização)
- WO/Desistência: +Z pontos (configurado na organização)

**Nota**: Os torneios herdam as configurações de pontuação da organização, garantindo consistência entre ranking e torneios.

**Exemplo**:
- Organização configura: Vitória +15, Derrota -8, WO -25
- Todos os torneios da organização usam essas regras automaticamente

### RN4.2 - Fases de Torneio
**Tipos Suportados**:

1. **Ladder Contínuo** (Ranking)
   - Atletas continuam competindo indefinidamente
   - Ranking atualizado após cada resultado
   - Sem "fim" formal, apenas pausar

2. **Round-Robin** (Todos vs Todos)
   - Cada atleta enfrenta todo outro uma vez
   - N atletas = N×(N-1)/2 confrontos
   - Ranking final por pontos acumulados

3. **Phase Dupla** (Grupos + Mata)
   - Fase 1: Grupos (4-6 atletas cada)
   - Top 2 de cada grupo avança
   - Fase 2: Eliminatória simples (oitavas)

4. **Série Melhor de 3/5/7**
   - Primeiro a vencer X jogos avança
   - Requer mínimo (2X-1) encontros

### RN4.3 - Promoção Entre Categorias
**Condição**: Quando um atleta atinge certo patamar.

**Trigger** (Configurável):
- Rating > 1500 (exemplo)
- Mínimo 20 confrontos realizados
- Taxa de vitória > 65%

**Sanção**: Possivelidade de "elevar" atleta para categoria superior

**Efeito**:
- Rating mantido
- Histórico preservado
- Notificação ao atleta
- Reinício opcional em categoria nova

### RN4.4 - Regra de Encerramento de Torneio
**Pré-requisitos para Encerramento**:
- Todos os confrontos previstos realizados OU
- Data de fim do torneio atingida OU
- Admin força encerramento

**Cálculo de Resultado Final**:
- Ranking por pontos acumulados
- Aplicar desempates (RN3.4)
- Gerar certificados (top 3, por exemplo)

**Arquivamento**:
- Torneio marcado como "concluído"
- Dados históricos congelados
- Bloqueado de edições (exceto admin)

---

## 5. Regras de Transparência e Auditoria

### RN5.1 - Rastreabilidade de Alterações
**Tudo Deve Ser Auditado**:
- Alteração manual de ranking (motivo, quem, quando)
- Cancelamento de confronto (motivo, quem, data original)
- Alteração de resultado (antes/depois, motivo)
- Mudança de configuração de organização

**Retenção**: Mínimo 2 anos de logs

### RN5.2 - Transparência de Algoritmo
**Disponibilidade**:
- Qualquer sorteio gera relatório automaticamente
- Explicação legível: "Por que X vs Y foi sorteado?"
- Cálculos exatos expostos (proximidade, penalidades, scores)

**Acesso**: Admin e atletas interessados podem solicitar explicação

### RN5.3 - Proteção de Dados Sensíveis
**LGPD Compliance**:
- Dados pessoais (data nascimento) criptografados
- Logs não expõem senhas/tokens
- Direito de exclusão ("direito ao esquecimento")

---

## 6. Regras de Operação

### RN6.1 - Horários de Operação
**Momento de Sorteio Automático** (se ativado):
- Padrão: Toda segunda-feira 08:00 AM (horário org)
- Frequência: Configurável (diária, semanal, customizada)

### RN6.2 - Limites de Throttling
- Máximo 300 atletas por organização no MVP
- Máximo 10000 confrontos históricos por organização
- API: 1000 requests/hora por organização

### RN6.3 - Validação de Integridade
- Consistência de dados verificada diariamente
- Se anomalia detectada (ranking negativo, loops, etc): notifica admin, bloqueia operações
- Reportar anomalia com contexto completo

---

## Resumo de Regras por Categoria

| Categoria | Regras | Impacto |
|-----------|--------|--------|
| **Ranking** | RN1.1-1.5 | Como pontos são calculados e mantidos |
| **Sorteio** | RN2.1-2.4 | Algoritmo de pareamento automático |
| **Resultado** | RN3.1-3.4 | Validações e tratamento resultado |
| **Torneio** | RN4.1-4.4 | Estrutura e ciclo de vida |
| **Transparência** | RN5.1-5.3 | Auditoria e confiabilidade |
| **Operação** | RN6.1-6.3 | Limites técnicos e operacionais |