namespace Clyvo.PetCare.Api.Application.DTOs.Vacina;

public class VacinaResponseDto
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

    /// <summary>Indica se a próxima dose está atrasada (hoje > DataProximaDose).</summary>
    public bool DoseAtrasada => DataProximaDose.HasValue && DataProximaDose.Value.Date < DateTime.Today;
}
