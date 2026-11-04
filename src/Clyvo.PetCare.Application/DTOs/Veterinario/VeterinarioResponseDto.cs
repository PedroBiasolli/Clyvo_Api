using Clyvo.PetCare.Application.DTOs.Common;

namespace Clyvo.PetCare.Application.DTOs.Veterinario;

/// <summary>Representação de um veterinário devolvida pela API.</summary>
public class VeterinarioResponseDto : RecursoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Crmv { get; set; } = string.Empty;
    public string? Especialidade { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
