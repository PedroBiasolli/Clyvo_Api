using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Api.Domain.Entities;

/// <summary>
/// Responsável legal do pet — quem tem o vínculo com a clínica
/// e recebe orientações, lembretes e relatórios de saúde.
/// </summary>
public class Tutor
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(14)]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(60)]
    public string? Cidade { get; set; }

    [StringLength(2)]
    public string? Uf { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public bool AceitaNotificacoes { get; set; } = true;

    // Navegação
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
