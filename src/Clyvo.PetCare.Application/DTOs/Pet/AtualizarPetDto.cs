using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Pet;

/// <summary>Dados atualizáveis de um pet (sexo, nascimento e tutor são imutáveis).</summary>
public sealed class AtualizarPetDto
{
    [Required, StringLength(80, MinimumLength = 1)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Raca { get; set; } = string.Empty;

    [Range(0.1, 120.0)]
    public decimal PesoKg { get; set; }

    [Required]
    public PorteCachorro Porte { get; set; }

    [StringLength(20)]
    public string? Microchip { get; set; }

    public bool Castrado { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}
