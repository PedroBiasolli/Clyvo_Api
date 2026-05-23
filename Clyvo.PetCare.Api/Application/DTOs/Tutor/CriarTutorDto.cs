using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Api.Application.DTOs.Tutor;

public class CriarTutorDto
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório.")]
    [StringLength(14, MinimumLength = 11)]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(60)]
    public string? Cidade { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string? Uf { get; set; }

    public bool AceitaNotificacoes { get; set; } = true;
}
