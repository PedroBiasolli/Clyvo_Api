using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Consulta;

/// <summary>Dados para agendar uma consulta (o status inicial é Agendada).</summary>
public sealed class CriarConsultaDto
{
    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    public TipoConsulta Tipo { get; set; }

    [StringLength(1000)]
    public string? MotivoConsulta { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PetId deve ser um identificador válido.")]
    public int PetId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "VeterinarioId deve ser um identificador válido.")]
    public int VeterinarioId { get; set; }
}
