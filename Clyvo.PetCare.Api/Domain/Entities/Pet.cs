using System.ComponentModel.DataAnnotations;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Domain.Entities;

/// <summary>
/// Pet acompanhado pela Clyvo VET. Nesta primeira versão focada em
/// cachorros, mas modelado para evoluir para outras espécies.
/// </summary>
public class Pet
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Raça do cachorro (ex: Golden Retriever, SRD, Shih Tzu).</summary>
    [Required, StringLength(80)]
    public string Raca { get; set; } = string.Empty;

    [Required]
    public DateTime DataNascimento { get; set; }

    [Required]
    public Sexo Sexo { get; set; }

    /// <summary>Peso atual em quilogramas. Importante para dosagem e score de risco.</summary>
    [Range(0.1, 120.0)]
    public decimal PesoKg { get; set; }

    [Required]
    public PorteCachorro Porte { get; set; }

    [StringLength(20)]
    public string? Microchip { get; set; }

    public bool Castrado { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // FK + Navegação
    public int TutorId { get; set; }
    public Tutor? Tutor { get; set; }

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
    public ICollection<Vacina> Vacinas { get; set; } = new List<Vacina>();

    /// <summary>Idade calculada em anos a partir da data de nascimento.</summary>
    public int IdadeAnos => CalcularIdade();

    private int CalcularIdade()
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade < 0 ? 0 : idade;
    }
}
