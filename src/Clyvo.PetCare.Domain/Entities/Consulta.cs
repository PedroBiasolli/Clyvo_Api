using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Exceptions;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Atendimento clínico — peça central da jornada de continuidade do cuidado.
/// Cada consulta pode sugerir um retorno (<see cref="ProximoRetorno"/>) que
/// alimenta os lembretes preventivos enviados ao tutor.
/// </summary>
public class Consulta : EntityBase
{
    public const int MotivoTamanhoMaximo = 1000;
    public const int TextoClinicoTamanhoMaximo = 2000;

    private Consulta() { }

    public DateTime DataHora { get; private set; }
    public TipoConsulta Tipo { get; private set; }
    public StatusConsulta Status { get; private set; }
    public string? MotivoConsulta { get; private set; }
    public string? Diagnostico { get; private set; }
    public string? Prescricao { get; private set; }
    public string? Observacoes { get; private set; }

    /// <summary>
    /// Data sugerida para o retorno. Coração da continuidade do cuidado —
    /// base para o motor de lembretes ao tutor.
    /// </summary>
    public DateTime? ProximoRetorno { get; private set; }

    public decimal? ValorCobrado { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public int PetId { get; private set; }
    public Pet? Pet { get; private set; }

    public int VeterinarioId { get; private set; }
    public Veterinario? Veterinario { get; private set; }

    /// <summary>
    /// Agenda uma nova consulta. Veterinários inativos não recebem agendamentos.
    /// </summary>
    public static Consulta Agendar(DateTime dataHora, TipoConsulta tipo, string? motivo, Pet pet, Veterinario veterinario)
    {
        DomainGuard.NaoNulo(pet, "Pet");
        DomainGuard.NaoNulo(veterinario, "Veterinário");

        if (!veterinario.Ativo)
            throw new DomainException($"O veterinário {veterinario.Nome} está inativo e não pode receber agendamentos.");

        return new Consulta
        {
            DataHora = DomainGuard.DataInformada(dataHora, "Data/hora da consulta"),
            Tipo = DomainGuard.EnumDefinido(tipo, "Tipo de consulta"),
            Status = StatusConsulta.Agendada,
            MotivoConsulta = DomainGuard.TextoOpcional(motivo, "Motivo da consulta", MotivoTamanhoMaximo),
            Pet = pet,
            PetId = pet.Id,
            Veterinario = veterinario,
            VeterinarioId = veterinario.Id,
            DataCriacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Registra a evolução do atendimento (status, diagnóstico, prescrição, retorno).
    /// Regras: consulta cancelada não é reaberta; consulta realizada exige diagnóstico;
    /// o retorno precisa ser posterior à consulta; valor cobrado não pode ser negativo.
    /// </summary>
    public void Atualizar(
        DateTime dataHora, TipoConsulta tipo, StatusConsulta status,
        string? motivo, string? diagnostico, string? prescricao, string? observacoes,
        DateTime? proximoRetorno, decimal? valorCobrado)
    {
        DomainGuard.DataInformada(dataHora, "Data/hora da consulta");
        DomainGuard.EnumDefinido(tipo, "Tipo de consulta");
        DomainGuard.EnumDefinido(status, "Status da consulta");

        if (Status == StatusConsulta.Cancelada && status != StatusConsulta.Cancelada)
            throw new DomainException("Consulta cancelada não pode ser reaberta. Agende uma nova consulta.");

        if (proximoRetorno.HasValue && proximoRetorno.Value <= dataHora)
            throw new DomainException("A data de retorno deve ser posterior à data da consulta.");

        if (valorCobrado < 0)
            throw new DomainException("O valor cobrado não pode ser negativo.");

        var diagnosticoNormalizado = DomainGuard.TextoOpcional(diagnostico, "Diagnóstico", TextoClinicoTamanhoMaximo);

        if (status == StatusConsulta.Realizada && diagnosticoNormalizado is null)
            throw new DomainException("Consultas realizadas exigem o registro do diagnóstico.");

        DataHora = dataHora;
        Tipo = tipo;
        Status = status;
        MotivoConsulta = DomainGuard.TextoOpcional(motivo, "Motivo da consulta", MotivoTamanhoMaximo);
        Diagnostico = diagnosticoNormalizado;
        Prescricao = DomainGuard.TextoOpcional(prescricao, "Prescrição", TextoClinicoTamanhoMaximo);
        Observacoes = DomainGuard.TextoOpcional(observacoes, "Observações", TextoClinicoTamanhoMaximo);
        ProximoRetorno = proximoRetorno;
        ValorCobrado = valorCobrado;
    }

    /// <summary>Consultas realizadas fazem parte do prontuário e não podem ser apagadas.</summary>
    public bool PodeSerRemovida() => Status != StatusConsulta.Realizada;

    public void GarantirQuePodeSerRemovida()
    {
        if (!PodeSerRemovida())
            throw new DomainException("Consultas realizadas não podem ser removidas — altere o status para Cancelada.");
    }
}
