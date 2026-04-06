# Casos de Uso - ClubeRank

## Visão Geral

Esta seção descreve os casos de uso principais da plataforma ClubeRank, com atores, pré-condições, fluxos principais, exceções e critérios de sucesso.

---

## 1. Criar Organização
**Ator**: Usuário anônimo / SuperAdmin

**Pré-condições**:
- O usuário possui email válido
- A organização ainda não existe

**Fluxo Principal**:
1. Usuário preenche nome, email, telefone, modalidade e plano
2. Sistema valida dados e cria organização
3. Sistema cria usuário administrador padrão
4. Sistema envia email de ativação
5. Organização é criada com configurações padrão

**Exceções**:
- Email já em uso -> mensagem de erro
- Dados obrigatórios faltando -> validação por campo

---

## 2. Cadastrar Atleta
**Ator**: AdminClube / GestorAtletas

**Pré-condições**:
- Organização ativa
- Usuário autenticado com permissão

**Fluxo Principal**:
1. Usuário seleciona formulário de cadastro de atleta
2. Informa nome, gênero, email opcional, telefone opcional e categoria
3. Sistema valida categoria e unicidade de email por organização
4. Sistema cria atleta com pontuação inicial configurada
5. Sistema registra auditoria de criação

**Exceções**:
- Categoria inválida -> mensagem de validação
- Email duplicado na organização -> mensagem de erro

---

## 3. Listar e Filtrar Atletas
**Ator**: AdminClube / GestorAtletas / Auditor

**Pré-condições**:
- Organização ativa
- Usuário autenticado

**Fluxo Principal**:
1. Usuário acessa a tela de atletas
2. Sistema carrega lista com paginação
3. Usuário aplica filtros por categoria, status, ranking
4. Sistema exibe resultados filtrados

**Exceções**:
- Filtro inválido -> retorno sem alteração
- Lista vazia -> mensagem de sem resultados

---

## 4. Criar Torneio
**Ator**: AdminClube / Organizador

**Pré-condições**:
- Organização ativa
- Categoria válida selecionada
- Pelo menos 2 atletas elegíveis cadastrados

**Fluxo Principal**:
1. Usuário preenche detalhes do torneio
2. Seleciona categoria e atletas participantes
3. Sistema valida regras de categoria e configurações de pontos
4. Sistema cria torneio e associa atletas
5. Sistema registra auditoria

**Exceções**:
- Categoria inválida -> validação
- Menos de 2 atletas na seleção -> erro

---

## 5. Gerar Sorteio de Confrontos
**Ator**: Organizador / Sistema (agendado)

**Pré-condições**:
- Torneio criado e com atletas suficientes
- Atletas ativos disponíveis

**Fluxo Principal**:
1. Usuário solicita sorteio ou o sistema agenda automaticamente
2. Sistema filtra atletas elegíveis e lê histórico de confrontos
3. Sistema calcula pareamentos com base em proximidade, diversidade e repetição
4. Sistema cria confrontos agendados
5. Sistema notifica os atletas envolvidos

**Exceções**:
- Número ímpar de atletas -> um atleta fica sem confronto
- Conflitos de categoria -> validação falha

---

## 6. Registrar Resultado de Confronto
**Ator**: GestorConfrontos / Atleta (dependendo da permissão)

**Pré-condições**:
- Confronto existe e está em status agendado
- Usuário tem permissão de registro

**Fluxo Principal**:
1. Usuário seleciona confronto
2. Informa resultado, placar e notas
3. Sistema valida o resultado e atualiza o confronto
4. Sistema calcula os novos pontos do atleta(s)
5. Sistema atualiza ranking e grava histórico
6. Sistema envia notificações e registra auditoria

**Exceções**:
- Resultado inválido -> mensagem de erro
- Confronto já concluído -> operação proibida

---

## 7. Recalcular Ranking
**Ator**: AdminClube / Auditor

**Pré-condições**:
- Existe alteração de resultado ou correção necessária
- Registro de auditoria associado

**Fluxo Principal**:
1. Usuário identifica necessidade de recalcular
2. Seleciona evento ou conjunto de confrontos
3. Sistema recalcula pontuações afetadas
4. Sistema atualiza ranking e gera relatório de mudanças
5. Sistema notifica atletas impactados

**Exceções**:
- Não há confrontos válidos para recalcular -> mensagem
- Conflito de dados -> erro de consistência

---

## 8. Visualizar Ranking
**Ator**: AdminClube / GestorAtletas / Atleta / Auditor

**Pré-condições**:
- Atletas com pontuação existente

**Fluxo Principal**:
1. Usuário acessa tela de ranking
2. Sistema recupera ranking com filtros por categoria e sexo
3. Interface exibe posições, pontuações, variações e próximos confrontos

**Exceções**:
- Ranking vazio -> mensagem de orientação
- Filtros sem resultados -> estado de lista vazia

---

## 9. Exportar Relatório
**Ator**: AdminClube / Auditor

**Pré-condições**:
- Dados de confrontos ou ranking disponíveis

**Fluxo Principal**:
1. Usuário seleciona período e tipo de relatório
2. Sistema gera relatório em PDF ou Excel
3. Usuário faz download do arquivo

**Exceções**:
- Período inválido -> erro de validação
- Falha na geração do arquivo -> notificação de erro

---

## 10. Gerenciar Usuários e Permissões
**Ator**: SuperAdmin / AdminClube

**Pré-condições**:
- Usuário autenticado com perfil de gestão

**Fluxo Principal**:
1. Usuário acessa painel de administração
2. Cria, edita ou desativa usuários
3. Atribui perfil e permissões
4. Sistema valida regras de permissão e salva alterações
5. Sistema registra auditoria

**Exceções**:
- Email de usuário já registrado -> erro
- Permissão insuficiente -> bloqueio

---

## 11. Monitorar Auditoria
**Ator**: Auditor / AdminClube / SuperAdmin

**Pré-condições**:
- Ações críticas registradas no sistema

**Fluxo Principal**:
1. Usuário acessa histórico de auditoria
2. Aplica filtros por usuário, ação, período ou organização
3. Sistema exibe entradas com dados antes/depois
4. Usuário analisa e exporta dados se necessário

**Exceções**:
- Nenhum resultado para os filtros aplicados -> mensagem
- Falha na visualização de JSON -> fallback legível
