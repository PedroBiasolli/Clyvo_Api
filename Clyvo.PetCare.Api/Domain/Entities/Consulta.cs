using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Domain.Entities;

/// <summary>
/// Atendimento clínico — peça central da jornada de continuidade do cuidado.
/// Cada consulta pode sugerir um retorno (ProximoRetorno) que alimenta os
/// lembretes preventivos enviados ao tutor.
/// </summary>
public class Consulta
{
    public int Id { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    public TipoConsulta Tipo { get; set; }

    [Required]
    public StatusConsulta Status { get; set; } = StatusConsulta.Agendada;

    [StringLength(1000)]
    public string? MotivoConsulta { get; set; }

    [StringLength(2000)]
    public string? Diagnostico { get; set; }

    [StringLength(2000)]
    public string? Prescricao { get; set; }

    [StringLength(2000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Data sugerida para próxima consulta/retorno. Coração da continuidade
    /// do cuidado — base para o motor de lembretes ao tutor.
    /// </summary>
    public DateTime? ProximoRetorno { get; set; }

    public decimal? ValorCobrado { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // FKs
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }
}
