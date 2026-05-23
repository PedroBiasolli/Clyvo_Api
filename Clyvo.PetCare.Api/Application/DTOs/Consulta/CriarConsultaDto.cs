using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Application.DTOs.Consulta;

public class CriarConsultaDto
{
    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    public TipoConsulta Tipo { get; set; }

    [StringLength(1000)]
    public string? MotivoConsulta { get; set; }

    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarioId { get; set; }
}
