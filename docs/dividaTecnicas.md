# Dividas Tecnicas

Este arquivo concentra pendencias tecnicas identificadas durante a execucao da Fase 1 e que podem ser tratadas depois que o fluxo principal estiver completamente concluido.

## Backlog

- [x] Corrigir todos os avisos de nulabilidade no dominio (`CS8618`, `CS8602`, `CS8603`, `CS8604`, `CS8625`) para reduzir risco de erro em runtime.
- [x] Revisar construtores privados e inicializacao padrao das entidades e value objects usados pelo EF Core para evitar estados invalidos silenciosos.
- [x] Reavaliar o mapeamento de `Usuario` de dominio, que hoje ainda nao possui persistencia real integrada ao fluxo principal.
- [x] Unificar o modelo de usuario/autenticacao entre dominio e ASP.NET Identity para eliminar duplicidade conceitual.
- [ ] Sincronizar no modelo EF Core a regra de email unico por organizacao de `Atleta`, que nesta etapa entrou por migracao manual para evitar bloqueio do owned type `Email`.
- [ ] Revisar a estrategia de filtros globais por tenant no `ClubeRankDbContext` e validar cenarios administrativos que eventualmente precisem escapar do filtro.
- [x] Criar testes automatizados especificos para isolamento multi-tenant entre organizacoes diferentes.
- [x] Revisar o uso de `Guid.Empty` em operacoes de sistema e identificar onde deve existir um usuario tecnico/auditoria explicito.
- [ ] Revisar a consistencia de `TenantId` em toda nova entidade adicionada futuramente, mantendo sempre o tenant como a organizacao.
- [ ] Revisar a camada de repositorios para aplicar paginacao, ordenacao e filtros de forma mais robusta e com contratos mais claros.
- [ ] Melhorar a modelagem de auditoria para registrar antes/depois, autor da acao e contexto de operacao de forma padronizada.
- [ ] Criar migracoes complementares sempre que houver ajuste estrutural de mapeamento ou integridade relacional.
- [ ] Revisar se o `TenantMiddleware` deve aceitar excecoes adicionais alem de `/swagger` e `/health`.
- [x] Substituir validacoes provisorias ou incompletas marcadas por `TODO` no codigo por implementacoes definitivas.
- [ ] Padronizar o ambiente Node do time em `v20+` para eliminar o workaround atual do frontend, que embute `node@20` localmente para permitir Angular 17 em maquinas ainda com Node 16 global.

## Observacoes desta rodada

- O build do backend voltou a ficar limpo com `0` avisos e `0` erros.
- O fluxo de decaimento parou de usar `Guid.Empty` e passou a registrar um usuario tecnico explicito em `SystemUsers.RankingDecayUserId`.
- A regra de email unico por organizacao de `Atleta` continua garantida pela migracao e pelo fluxo de aplicacao, mas a sincronizacao completa no modelo EF Core segue pendente por conta da limitacao atual do owned type `Email`.
- O teste de integracao principal foi ajustado para nao depender da ordem de pareamento retornada pela API, evitando falso negativo quando `AtletaA` nao coincide com o primeiro atleta criado.
- O uso restante de `Guid.Empty` em tenant/usuario foi substituido por constantes nomeadas (`SystemTenants.GlobalTenantId` e `SystemUsers.AuditFallbackUserId`) ou por `default` apenas como valor temporario de `out`.
- O fluxo principal deixou de criar/persistir um `Usuario` de dominio paralelo; a autenticacao e o onboarding ficaram centralizados no ASP.NET Identity, e o dominio manteve apenas `UsuarioId` escalar para auditoria/historico.
