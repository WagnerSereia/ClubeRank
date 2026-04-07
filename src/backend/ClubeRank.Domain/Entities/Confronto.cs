using ClubeRank.Domain.Entities;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Domain.Entities;

public class Confronto : Entity
{
    public Guid AtletaAId { get; private set; }
    public Guid AtletaBId { get; private set; }
    public Guid TorneioId { get; private set; }
    public StatusConfronto Status { get; private set; }
    public ResultadoConfronto? Resultado { get; private set; }
    public DateTime DataAgendamento { get; private set; }
    public string? Notas { get; private set; }

    // Navegação
    public Atleta AtletaA { get; private set; } = null!;
    public Atleta AtletaB { get; private set; } = null!;
    public Torneio Torneio { get; private set; } = null!;

    private Confronto() { } // Para EF Core

    public Confronto(Guid atletaAId, Guid atletaBId, Guid torneioId, Guid tenantId, DateTime dataAgendamento)
    {
        if (atletaAId == atletaBId)
            throw new ArgumentException("Atleta A e B devem ser diferentes");

        AtletaAId = atletaAId;
        AtletaBId = atletaBId;
        TorneioId = torneioId;
        Status = StatusConfronto.Agendado;
        DataAgendamento = dataAgendamento;
        TenantId = tenantId; // Tenant sempre representa a organização
    }

    public void RegistrarResultado(ResultadoConfronto resultado)
    {
        if (Status != StatusConfronto.Agendado)
            throw new InvalidOperationException("Apenas confrontos agendados podem ter resultado registrado");

        Resultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
        Status = StatusConfronto.Realizado;
        Atualizar();
    }

    public void Cancelar()
    {
        if (Status == StatusConfronto.Realizado)
            throw new InvalidOperationException("Confrontos realizados não podem ser cancelados");

        Status = StatusConfronto.Cancelado;
        Atualizar();
    }

    public void Reagendar(DateTime novaData)
    {
        if (Status != StatusConfronto.Agendado)
            throw new InvalidOperationException("Apenas confrontos agendados podem ser reagendados");

        DataAgendamento = novaData;
        Atualizar();
    }

    public void AdicionarNotas(string notas)
    {
        Notas = notas;
        Atualizar();
    }
}
