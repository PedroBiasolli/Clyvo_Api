using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Application.DTOs.Pet;

public class CriarPetDto
{
    [Required, StringLength(80, MinimumLength = 1)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Raca { get; set; } = string.Empty;

    [Required]
    public DateTime DataNascimento { get; set; }

    [Required]
    public Sexo Sexo { get; set; }

    [Range(0.1, 120.0, ErrorMessage = "Peso deve estar entre 0.1 e 120kg.")]
    public decimal PesoKg { get; set; }

    [Required]
    public PorteCachorro Porte { get; set; }

    [StringLength(20)]
    public string? Microchip { get; set; }

    public bool Castrado { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    [Required]
    public int TutorId { get; set; }
}
