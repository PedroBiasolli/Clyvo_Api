using System.ComponentModel.DataAnnotations;

namespace Clyvo.PetCare.Api.Domain.Entities;

/// <summary>
/// Médico veterinário responsável por consultas, prescrições e vacinas.
/// </summary>
public class Veterinario
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CRMV (registro do Conselho Regional de Medicina Veterinária).</summary>
    [Required, StringLength(20)]
    public string Crmv { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Especialidade { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
    public ICollection<Vacina> Vacinas { get; set; } = new List<Vacina>();
}
