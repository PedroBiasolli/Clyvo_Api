using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Api.Domain.Entities;

/// <summary>
/// Carteira de vacinação digital do pet. Centraliza o histórico vacinal
/// (V8/V10, Antirrábica, Tosse dos canis, Giárdia, Gripe canina) e
/// alimenta o motor preventivo via DataProximaDose.
/// </summary>
public class Vacina
{
    public int Id { get; set; }

    /// <summary>Nome da vacina (V8, V10, Antirrábica, Giárdia, etc).</summary>
    [Required, StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public DateTime DataAplicacao { get; set; }

    /// <summary>Data prevista para próxima dose/reforço.</summary>
    public DateTime? DataProximaDose { get; set; }

    [StringLength(50)]
    public string? Lote { get; set; }

    [StringLength(80)]
    public string? Fabricante { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

    // FKs
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public int? VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }
}
