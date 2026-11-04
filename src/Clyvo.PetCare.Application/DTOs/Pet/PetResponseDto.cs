using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Pet;

/// <summary>Representação de um pet devolvida pela API.</summary>
public class PetResponseDto : RecursoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public int IdadeAnos { get; set; }
    public Sexo Sexo { get; set; }
    public string SexoDescricao => Sexo == Sexo.Macho ? "Macho" : "Fêmea";
    public decimal PesoKg { get; set; }
    public PorteCachorro Porte { get; set; }
    public string PorteDescricao => Porte.ToString();
    public string? Microchip { get; set; }
    public bool Castrado { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
    public int TutorId { get; set; }
    public string? TutorNome { get; set; }
}
