using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Api.Application.DTOs.Veterinario;

public class AtualizarVeterinarioDto
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
