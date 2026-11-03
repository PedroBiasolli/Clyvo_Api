using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Application.DTOs.Veterinario;

/// <summary>Dados atualizáveis de um veterinário (o CRMV é imutável).</summary>
public sealed class AtualizarVeterinarioDto
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Especialidade { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; }
}
