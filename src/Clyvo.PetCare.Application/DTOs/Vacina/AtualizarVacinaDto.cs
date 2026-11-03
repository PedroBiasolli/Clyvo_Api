using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Application.DTOs.Vacina;

/// <summary>Correção de um registro de vacina.</summary>
public sealed class AtualizarVacinaDto
{
    [Required, StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public DateTime DataAplicacao { get; set; }

    public DateTime? DataProximaDose { get; set; }

    [StringLength(50)]
    public string? Lote { get; set; }

    [StringLength(80)]
    public string? Fabricante { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public int? VeterinarioId { get; set; }
}
