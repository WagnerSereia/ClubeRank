namespace ClubeRank.Domain.ValueObjects;

public enum Genero
{
    Masculino,
    Feminino,
    Outro
}

public enum StatusAtleta
{
    Ativo,
    Inativo,
    Suspenso
}

public enum Categoria
{
    A,
    B,
    C,
    Iniciante,
    Especial
}

public enum StatusOrganizacao
{
    Ativa,
    Suspensa,
    Inativa
}

public enum TipoPlano
{
    Basico,
    Profissional,
    Empresarial
}

public enum TipoTorneio
{
    Ladder,
    RoundRobin,
    Eliminatoria,
    Serie
}

public enum StatusTorneio
{
    Planejado,
    Ativo,
    Encerrado,
    Cancelado
}

public enum StatusConfronto
{
    Agendado,
    Realizado,
    Cancelado
}

public enum PerfilUsuario
{
    SuperAdmin,
    AdminOrganizacao,
    GestorAtletas,
    GestorConfrontos,
    Auditor
}

public enum StatusUsuario
{
    Ativo,
    Inativo,
    Bloqueado
}