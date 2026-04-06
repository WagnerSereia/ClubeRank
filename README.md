# ClubeRank

ClubeRank é uma plataforma SaaS para gestão de rankings, torneios e confrontos esportivos com foco em transparência, simplicidade e escalabilidade.

## Status Atual

O projeto está na fase de **especificação e planejamento**. Os documentos em `/spec` foram revisados e expandidos para fornecer uma base sólida antes de iniciar a implementação.

## Fonte de Verdade

Todos os requisitos, regras de negócio, arquitetura, modelos, casos de uso e planos estão em:
- `/spec/01-visao-produto.md`
- `/spec/02-requisitos-funcionais.md`
- `/spec/03-regras-negocio.md`
- `/spec/04-requisitos-nao-funcionais.md`
- `/spec/05-arquitetura-solucao.md`
- `/spec/06-modelo-dominio.md`
- `/spec/07-modelo-dados-inicial.md`
- `/spec/08-casos-uso.md`
- `/spec/09-perfis-permissoes.md`
- `/spec/10-roadmap-tecnico.md`
- `/spec/11-diretrizes-codex.md`
- `/spec/12-plano-execucao.md`

## Próximo Passo

A implementação deve seguir o plano de execução descrito em `/spec/12-plano-execucao.md`. Antes de gerar código, revise e valide os documentos de especificação.

## Estrutura Sugerida do Repositório

- `/src/backend` - Backend em .NET 8 com Clean Architecture
- `/src/frontend` - Frontend em Angular 17
- `/infra` - Configuração Docker, Docker Swarm, nginx e Portainer
- `/spec` - Documentação de requisitos e planejamento

## Tecnologias Planejadas

- Backend: .NET 8, EF Core, PostgreSQL
- Frontend: Angular 17, Angular Material, NgRx
- Deploy: Docker Swarm single node, nginx reverse proxy
- Observabilidade: Portainer, Prometheus, Grafana
- Autenticação: JWT

## Como Começar

1. Revise os arquivos em `/spec`.
2. Confirme o plano de implementação com a equipe.
3. Configure o ambiente local de desenvolvimento.
4. Inicie pela fase 0 do roadmap técnico.

## Observações

- `/spec` é a fonte de verdade. Modificações devem ser refletidas ali primeiro.
- Não iniciar implementação antes de validar os documentos técnicos principais.
- Foco no MVP: simplicidade, segurança e multi-tenancy.
