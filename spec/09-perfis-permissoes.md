# Perfis e Permissões - ClubeRank

## Visão Geral

Este documento descreve os perfis de usuário e as permissões associadas, garantindo controle de acesso granular e alinhado ao modelo de negócio.

---

## Perfis Principais

### SuperAdmin
**Escopo**: Global

**Permissões**:
- Criar e gerenciar organizações
- Gerenciar todos os usuários
- Visualizar auditoria de todas as organizações
- Acessar relatórios globais
- Configurar políticas de segurança

### AdminClube
**Escopo**: Organização

**Permissões**:
- Gerenciar atletas
- Criar, editar e encerrar torneios
- Configurar regras de pontuação e categorias
- Visualizar dashboards e relatórios da organização
- Gerenciar usuários internos (GestorAtletas, GestorConfrontos, Atleta)

### Organizador
**Escopo**: Organização

**Permissões**:
- Criar torneios e fases
- Gerar sorteios de confrontos
- Visualizar e ajustar confrontos agendados
- Acompanhar resultados e classificações

### GestorAtletas
**Escopo**: Organização

**Permissões**:
- Cadastrar e editar atletas
- Inativar/reativar atletas
- Visualizar rankings e histórico de atletas
- Exportar listas de atletas

### GestorConfrontos
**Escopo**: Organização

**Permissões**:
- Registrar resultados de confrontos
- Editar resultados dentro do prazo permitido
- Anular confrontos com justificativa
- Visualizar confrontos pendentes e realizados

### Atleta
**Escopo**: Organização

**Permissões**:
- Visualizar próprio ranking
- Visualizar próximos confrontos e histórico
- Receber notificações in-app e por email
- Acessar relatórios pessoais simples

### Auditor
**Escopo**: Organização

**Permissões**:
- Visualizar logs de auditoria
- Consultar histórico de ações críticas
- Exportar relatórios de auditoria
- Não pode alterar dados de negócio

---

## Matriz de Permissões

| Permissão | SuperAdmin | AdminClube | Organizador | GestorAtletas | GestorConfrontos | Atleta | Auditor |
|---|---|---|---|---|---|---|---|
| Criar organização | X |   |   |   |   |   |   |
| Gerenciar usuários | X | X |   |   |   |   |   |
| Cadastrar atleta | X | X |   | X |   |   |   |
| Editar atleta | X | X |   | X |   |   |   |
| Inativar/reactivar atleta | X | X |   | X |   |   |   |
| Criar torneio | X | X | X |   |   |   |   |
| Gerar sorteio | X | X | X |   |   |   |   |
| Registrar resultado | X | X | X |   | X |   |   |
| Editar resultado | X | X | X |   | X |   |   |
| Anular confronto | X | X | X |   | X |   |   |
| Visualizar ranking | X | X | X | X | X | X | X |
| Exportar relatórios | X | X | X |   |   |   | X |
| Visualizar auditoria | X | X |   |   |   |   | X |
| Configurar regras de pontuação | X | X |   |   |   |   |   |
| Acessar dashboard | X | X | X | X | X | X |   |

---

## Regras de Autorização

- **SuperAdmin** tem acesso global e promove organizações
- **AdminClube** controla a organização inteira e suas configurações
- **Organizador** foca em torneios e confrontos
- **GestorAtletas** foca na operação de atletas
- **GestorConfrontos** foca no lançamento e ajuste de resultados
- **Atleta** tem acesso somente a sua própria informação
- **Auditor** tem acesso somente leitura a logs e históricos

---

## Considerações de Implementação

- A autorização deve ser feita por perfil e por recurso
- Perfis podem ser combinados em usuários com múltiplas permissões
- A verificação de `OrganizacaoId` é obrigatória para todos os perfis organizacionais
- Perfis globais (`SuperAdmin`) não devem ser restringidos por `OrganizacaoId`
- Logs de autorização devem ser auditáveis para ações administrativas