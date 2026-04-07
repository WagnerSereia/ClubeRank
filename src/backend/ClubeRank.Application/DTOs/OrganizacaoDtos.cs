using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Application.DTOs;

public record CriarOrganizacaoDto(
    string Nome,
    string Email,
    string? Telefone,
    string Modalidade,
    TipoPlano Plano
);

public record OnboardingOrganizacaoDto(
    string Nome,
    string Email,
    string? Telefone,
    string Modalidade,
    TipoPlano Plano,
    string NomeAdministrador,
    string SenhaAdministrador
);

public record OrganizacaoDto(
    Guid Id,
    string Nome,
    string Email,
    string? Telefone,
    string Modalidade,
    StatusOrganizacao Status,
    TipoPlano Plano,
    string? LogoUrl,
    DateTime DataCriacao
);

public record OrganizacaoOnboardingResponseDto(
    Guid OrganizacaoId,
    Guid TenantId,
    string NomeOrganizacao,
    string EmailAdministrador,
    string PerfilAdministrador
);
