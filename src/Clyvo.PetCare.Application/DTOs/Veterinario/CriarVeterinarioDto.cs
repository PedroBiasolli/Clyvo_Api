using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Application.DTOs.Veterinario;

/// <summary>Dados para cadastrar um veterinário (o CRMV deve ser único).</summary>
public sealed class CriarVeterinarioDto
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 4)]
    public string Crmv { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Especialidade { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }
}
