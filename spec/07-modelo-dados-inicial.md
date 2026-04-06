# Modelo de Dados Inicial - ClubeRank

## Visão Geral

O modelo de dados adota uma estrutura relacional multi-tenant orientada a PostgreSQL/SQL Server e mapeamento via EF Core. Cada organização mantém seus próprios dados isolados por `OrganizacaoId`.

---

## Entidades Principais

### 1. Organizacao
- **OrganizacaoId** (PK, GUID)
- Nome (string, 200)
- Email (string, 200)
- Telefone (string, 50)
- Modalidade (string, 100)
- Plano (string, 50)
- Status (string, 30)
- DataCadastro (datetime)
- ConfiguracaoId (FK)

### 2. ConfiguracaoOrganizacao
- **ConfiguracaoId** (PK, GUID)
- OrganizacaoId (FK)
- PontosVitoria (int)
- PontosDerrota (int)
- PontosDerrotaWO (int)
- PontuacaoInicial (int)
- DiasDecaimento (int)
- PontosDecaimento (int)
- TempoCacheRankingSegundos (int)

### 3. Usuario
- **UsuarioId** (PK, GUID)
- OrganizacaoId (FK nullable, para SuperAdmin global)
- Nome (string, 200)
- Email (string, 200)
- SenhaHash (string, 256)
- Perfil (string, 50)
- Status (string, 30)
- UltimoAcesso (datetime nullable)
- DataCadastro (datetime)

### 4. Atleta
- **AtletaId** (PK, GUID)
- OrganizacaoId (FK)
- Nome (string, 200)
- Genero (string, 30)
- Email (string, 200 nullable)
- Telefone (string, 50 nullable)
- Categoria (string, 50)
- Status (string, 30)
- PontuacaoAtual (int)
- PosicaoAtual (int nullable)
- DataCadastro (datetime)

### 5. Torneio
- **TorneioId** (PK, GUID)
- OrganizacaoId (FK)
- Nome (string, 200)
- Descricao (text nullable)
- Tipo (string, 50)
- Status (string, 30)
- Categoria (string, 50 nullable)
- DataInicio (datetime nullable)
- DataFim (datetime nullable)
- PontosVitoria (int nullable)
- PontosDerrota (int nullable)
- PontosDerrotaWO (int nullable)
- DataCadastro (datetime)

### 6. TorneioAtleta
- **TorneioAtletaId** (PK, GUID)
- TorneioId (FK)
- AtletaId (FK)
- DataInclusao (datetime)
- Status (string, 30)

### 7. Confronto
- **ConfrontoId** (PK, GUID)
- TorneioId (FK)
- AtletaAId (FK)
- AtletaBId (FK)
- Status (string, 30)
- ResultadoId (FK nullable)
- DataRegistro (datetime nullable)
- Notas (text nullable)
- DataCadastro (datetime)

### 8. ResultadoConfronto
- **ResultadoId** (PK, GUID)
- ConfrontoId (FK)
- Vencedor (string, 20)
- TipoResultado (string, 20)
- Placar (string, 100 nullable)
- PontosAtletaA (int)
- PontosAtletaB (int)
- Observacao (text nullable)
- DataRegistro (datetime)

### 9. HistoricoRanking
- **HistoricoRankingId** (PK, GUID)
- AtletaId (FK)
- OrganizacaoId (FK)
- PontuacaoAnterior (int)
- PontuacaoAtual (int)
- Variacao (int)
- DataAlteracao (datetime)
- Motivo (string, 200)
- ResponsavelId (FK)

### 10. Auditoria
- **AuditoriaId** (PK, GUID)
- OrganizacaoId (FK nullable)
- UsuarioId (FK nullable)
- Entidade (string, 100)
- EntidadeId (string, 100)
- Acao (string, 100)
- DadosAntes (json nullable)
- DadosDepois (json nullable)
- DataAcao (datetime)
- IP (string, 50 nullable)

---

## Relacionamentos

- **Organizacao 1:N Usuario**
- **Organizacao 1:N Atleta**
- **Organizacao 1:N Torneio**
- **Organizacao 1:N HistoricoRanking**
- **Organizacao 1:N Auditoria**
- **Torneio 1:N Confronto**
- **Torneio 1:N TorneioAtleta**
- **Atleta 1:N TorneioAtleta**
- **Atleta 1:N Confronto** (como AtletaA ou AtletaB)
- **Confronto 1:1 ResultadoConfronto**

---

## Índices e Constraints

- Index em `OrganizacaoId` para todas as tabelas multi-tenant
- Index em `Usuario.Email` com unicidade global
- Index em `Atleta.Email` com unicidade por organização
- Index em `Torneio.Status`, `Confronto.Status`, `Atleta.Status`
- Constraint `Categoria` com valores permitidos: A, B, C, Iniciante, Especial, Masculino, Feminino, Outro
- Constraint `Status` com valores padronizados
- Constraint `PontosVitoria`, `PontosDerrota`, `PontosDerrotaWO` não negativos quando definidos

---

## Implementação com EF Core

### Estratégia de Mapeamento
- Cada entidade mapeada para uma tabela correspondente
- `Owned Types` para objetos de valor como `Email` e `Telefone`
- `Query Filters` no DbContext para `OrganizacaoId` em multi-tenancy
- `Shadow Properties` para auditoria de criação e modificação

### Esquema Inicial
```sql
CREATE TABLE Organizacao (
  OrganizacaoId uuid PRIMARY KEY,
  Nome varchar(200) NOT NULL,
  Email varchar(200) NOT NULL,
  Telefone varchar(50),
  Modalidade varchar(100) NOT NULL,
  Plano varchar(50) NOT NULL,
  Status varchar(30) NOT NULL,
  DataCadastro timestamp NOT NULL
);
```