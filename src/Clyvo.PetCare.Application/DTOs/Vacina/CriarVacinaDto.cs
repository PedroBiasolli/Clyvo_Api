using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Application.DTOs.Vacina;

/// <summary>Registro de aplicação de vacina na carteira digital do pet.</summary>
public sealed class CriarVacinaDto
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

    [Range(1, int.MaxValue, ErrorMessage = "PetId deve ser um identificador válido.")]
    public int PetId { get; set; }

    public int? VeterinarioId { get; set; }
}
