# Fase 1 - MVP do ClubeRank

## Objetivo
Entregar o núcleo funcional de ClubeRank com o mínimo viável para gestão de organizações, atletas, torneios, confrontos e ranking. Esta fase deve permitir a operação de um clube ou federação com controle de participantes, geração de confrontos e atualização de resultados em um sistema multi-tenant.

## Fonte de verdade
Use os artefatos em `/spec` como fonte única de verdade. Os principais documentos para esta fase são:
- `spec/01-visao-produto.md`
- `spec/02-requisitos-funcionais.md`
- `spec/03-regras-negocio.md`
- `spec/05-arquitetura-solucao.md`
- `spec/06-modelo-dominio.md`
- `spec/08-casos-uso.md`

## Escopo desta fase
### Funcionalidades principais
1. Gestão de organizações multi-tenant
   - Cadastro de organização
   - Criação de usuário administrador padrão
   - Isolamento de dados por tenant
2. Autenticação e autorização básica
   - Login de usuário
   - Perfis iniciais: Super Admin, Admin Organização, Gestor de Atletas, Gestor de Confrontos, Auditor
   - Controle de acesso por perfil
3. Cadastro e gestão de atletas
   - Criar, editar e listar atletas
   - Filtros por categoria, status e ranking
   - Inativar / reativar atleta
4. Gestão de rankings
   - Exibir ranking em tempo real
   - Visualizar histórico de confrontos por atleta
   - Atualizar ranking imediatamente após registro de resultado
5. Criação de torneios e configuração básica
   - Criar torneio com dados essenciais
   - Associar atletas ao torneio
   - Validar categorias e regras de pontuação
6. Geração de confrontos
   - Gerar confrontos para torneios do tipo ladder e round-robin simples
   - Aplicar critérios de proximidade de ranking e evitar repetição quando possível
7. Registro de resultados de confrontos
   - Registrar vitória, derrota, empate e WO
   - Atualizar ranking e histórico de atletas
   - Validar resultado somente para confrontos agendados
8. Auditoria básica
   - Registrar quem realizou criação de atletas, torneios, confrontos e resultados
   - Disponibilizar histórico de ações

### Não incluído nesta fase
- Integrações externas (calendário, redes sociais)
- Exportação avançada de relatórios
- PWA ou offline
- Regras de pontuação complexas além do básico configurável
- Modalidades de torneio avançadas (eliminatórias com fases múltiplas, série melhor de N)

## Detalhamento do escopo
### Gestão de organizações
- Criar organização com nome, email, telefone, modalidade e plano
- Validar obrigatoriedade de campos
- Garantir organização isolada por tenant
- Criar usuário administrador com perfil padrão

### Atletas
- Capturar: nome completo, gênero, email, telefone, categoria
- Definir pontuação inicial padrão de `1000` pontos
- Garantir email único por organização
- Implementar filtros e paginação na listagem
- Status de atleta: `Ativo`, `Inativo`, `Suspenso`

### Ranking
- Obter ranking ordenado por pontuação desc
- Exibir variação de pontuação e próximo confronto agendado
- Permitir filtro por categoria, sexo e período
- Mostrar histórico de confrontos do atleta com data, adversário, resultado, pontuação antes/depois

### Torneios e confrontos
- Criar torneio com: nome, tipo (`Ladder`, `RoundRobin`), categoria, data de início/fim, pontuação por vitória/derrota/WO
- Associar atletas elegíveis ao torneio
- Gerar confrontos usando algoritmo inicial baseado em proximidade de ranking
- Garantir que cada atleta tenha no máximo 1 confronto por dia
- Validar número mínimo de atletas (≥2)

### Registro de resultados
- Registrar resultado para confronto agendado
- Suportar tipos: vitória, derrota, empate, WO
- Exigir justificativa para WO
- Atualizar ranking imediatamente e registrar auditoria
- Bloquear registro duplicado de resultado

## Regras de negócio essenciais
- Pontuação padrão: vitória +10, derrota -5, WO -20
- Rating inicial padrão: 1000
- Limite de pontuação: mínimo 0, máximo 5000
- Variação mínima de ranking: ±1 ponto
- Atleta sem confronto por 30 dias sofre decaimento de -10 pontos a cada semana subsequente até um piso de 900 pontos
- Critérios de pareamento:
  - proximidade de pontuação
  - evitar confrontos repetidos quando houver alternativas
  - priorizar adversários nunca enfrentados

## Caso de uso principal
- Usuário administrador cria organização e atletas
- Cria torneio e associa atletas
- Gera confrontos automaticamente
- Registra resultado de confronto
- Sistema atualiza ranking e exibe histórico

## Critérios de aceitação
- Sistema cria organização e usuário administrador com validação
- Atletas podem ser cadastrados e listados com filtro e paginação
- Torneio pode ser criado somente com atletas válidos e categoria compatível
- Confronto gerado respeita critérios de proximidade e evita repetição
- Resultado de confronto atualiza ranking corretamente e persiste histórico
- Auditoria armazena operações de criação/edição e permite consulta

## Artefatos esperados para implementação
- APIs REST documentadas com contratos claros para criação de organização, atletas, torneios, confrontos, resultados e ranking
- Frontend com telas mínimas para fluxo completo de MVP
- Modelo de domínio inicial com entidades: `Organizacao`, `Usuario`, `Atleta`, `Torneio`, `Confronto`, `Ranking`
- Validações de negócio implementadas no domínio, não apenas no frontend
- Estrutura básica de autorização RBAC para perfis definidos

## Observações importantes
1. Use `/spec` como documento autoritativo para regras e requisitos.
2. Fase 1 deve priorizar estabilidade do fluxo principal sobre recursos complementares.
3. Não gerar código nesta fase de definição: este documento deve ser usado como briefing para desenvolvimento.
