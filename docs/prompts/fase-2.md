# Fase 2 - Expansão de funcionalidades e refinamento

## Objetivo
Entregar funcionalidades adicionais que ampliem o escopo do MVP e deixem o produto pronto para uso comercial mais amplo, com maior controle de torneios, permissões de usuários, relatórios e notificações.

## Fonte de verdade
Os requisitos desta fase devem ser alinhados com os documentos do `/spec` como fonte de verdade, principalmente:
- `spec/02-requisitos-funcionais.md`
- `spec/03-regras-negocio.md`
- `spec/06-modelo-dominio.md`
- `spec/08-casos-uso.md`
- `spec/05-arquitetura-solucao.md`

## Escopo desta fase
### Funcionalidades adicionais
1. Configuração de organização avançada
   - Parâmetros de ranking customizáveis
   - Regras de decaimento e pontos
   - Limites do plano atual (quantidade de atletas, storage)
2. Permissões e gestão de usuários
   - CRUD de usuários internos
   - Atribuição de perfis e papéis
   - Painel de administração de acesso
3. Tipos de torneio avançados
   - Round-robin completo
   - Eliminatória simples (mata-mata)
   - Fases de grupo + eliminatória
   - Séries melhor de N (best of 3/5/7)
4. Geração de fases e tabelas
   - Criar fases para torneios complexos
   - Gerar chave eliminatória e próximas fases automaticamente
   - Suporte a confrontos programados e reorganização de tabelas
5. Notificações de confronto e resultados
   - Notificações in-app
   - Envio de email para atletas e gestores
   - Alertas de confronto agendado e resultado registrado
6. Relatórios e exportação
   - Exportar lista de atletas e ranking para CSV/Excel
   - Relatórios de torneio e confrontos
   - Exportar histórico de auditoria
7. Recalculo de ranking e correção
   - Recalcular ranking após correção ou anulação
   - Reportar mudanças de ranking afetadas
   - Aprovação de alterações importantes
8. Dashboard operacional
   - Indicadores chave: número de atletas, torneios ativos, confrontos pendentes
   - Tabela de próximos confrontos e resultados recentes
   - Visão de performance do torneio

### Não incluído nesta fase
- Integrações complexas com terceiros além de email
- API pública externa para terceiros
- App mobile nativo e PWA completo (apenas responsividade aprimorada)
- Machine learning ou previsão de resultado

## Detalhamento do escopo
### Configuração de organização
- Permitir que o administrador ajuste:
  - pontos por vitória, derrota e WO
  - rating inicial padrão
  - regras de decaimento temporal
  - máximo de confrontos por atleta por período
- Exibir limites do plano e alertar quando próximo do limite

### Gestão de usuários e permissões
- Permitir criação, edição e exclusão de usuários
- Perfis adicionais: `Organizador`, `Auditor`
- Garantir que apenas usuários autorizados possam registrar resultados e criar torneios
- Validar regras RBAC no backend

### Torneios avançados
- Round-robin completo: cada atleta enfrenta todos os demais uma vez
- Eliminatória simples: gerar chave e avançar vencedores
- Fase de grupos: grupos de 4-6 atletas, avançam top N para fase seguinte
- Séries melhor de N: suportar melhor de 3, 5 ou 7 jogos

### Geração de fases e tabelas
- Criar fases em sequência e víncular ao torneio
- Gerar automaticamente confrontos da fase seguinte quando a fase anterior terminar
- Permitir reiniciar fase ou corrigir chave em caso de problema
- Exibir visualização de chave eliminatória e fases de grupos

### Notificações
- Notificar atletas sobre confronto agendado com: adversário, data e local
- Notificar quando resultado for registrado e ranking atualizado
- Implementar templates de email básicos
- Oferecer preferências de notificação in-app vs email

### Relatórios e exportações
- Exportar lista de atletas com filtros aplicados
- Exportar ranking completo por categoria
- Exportar histórico de confrontos e resultados
- Exportar auditoria de ações em CSV/Excel

### Recalculo de ranking
- Permitir correção de resultado com justificativa
- Recalcular ranking de atletas impactados e gerar relatório de mudanças
- Registrar auditoria da correção
- Notificar atletas afetados quando ranking for alterado

## Regras de negócio avançadas
- Torneio eliminatório exige chave balanceada e avançamento correto
- Rodadas de grupo devem obedecer ao número de atletas por grupo
- Empate em torneio com fase eliminatória deve ser tratado conforme regra do torneio (pode avançar por critério ou exigir penalidade)
- WO em torneio deve aplicar pontuação de WO e atualizar chaves corretamente
- Regras de desempate em ranking:
  - confrontos diretos
  - maior número de vitórias
  - menor número de WO
  - data de cadastro mais antiga

## Caso de uso principal
- Administrador ajusta configurações do clube e cria novos usuários
- Organizador cria torneio com fases e gera tabela de eliminatórias
- Sistema notifica atletas envolvidos em cada confronto
- Gestor registra resultados e, se necessário, corrige registro com justificativa
- Relatórios são exportados pelos auditores

## Critérios de aceitação
- Configurações de organização podem ser alteradas e têm efeito imediato nos cálculos de pontuação
- Usuários podem ser gerenciados com perfis e permissões, e o backend bloqueia operações não autorizadas
- Tipos de torneio avançados podem ser criados e executados sem falha
- Notificações são disparadas para confronto e resultado registrado
- Exportações geram arquivos válidos e completos
- Correções de resultado disparam recalculo e auditoria apropriada

## Artefatos esperados para implementação
- APIs adicionais para configuração de organização, gerenciamento de usuários, tipos de torneio e notificações
- Frontend com telas de administração avançada, fases de torneio e relatórios
- Modelos de domínio para fases, chaves eliminatórias e histórico de auditoria extendido
- Regras de negócio implementadas no domínio para todas as verificações de fluxo
- Documentação de uso e contratos de API atualizados

## Observações importantes
1. Esta fase deve seguir a estrutura arquitetural definida em `/spec/05-arquitetura-solucao.md`.
2. Priorizar segurança e controle de acesso ao expandir permissões.
3. O comportamento dos torneios avançados deve ser validado com casos de uso reais antes de implementação.
