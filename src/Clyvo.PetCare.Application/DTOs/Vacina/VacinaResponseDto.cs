using Clyvo.PetCare.Application.DTOs.Common;

namespace Clyvo.PetCare.Application.DTOs.Vacina;

/// <summary>Representação de um registro de vacina devolvida pela API.</summary>
public class VacinaResponseDto : RecursoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataAplicacao { get; set; }
    public DateTime? DataProximaDose { get; set; }
    public string? Lote { get; set; }
    public string? Fabricante { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataRegistro { get; set; }
    public int PetId { get; set; }
    public string? PetNome { get; set; }
    public int? VeterinarioId { get; set; }
    public string? VeterinarioNome { get; set; }

    /// <summary>Indica se a próxima dose já venceu (data de hoje posterior à próxima dose).</summary>
    public bool DoseAtrasada { get; set; }
}
