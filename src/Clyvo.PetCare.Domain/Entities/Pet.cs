using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Pet acompanhado pela Clyvo VET. Nesta versão focado em cachorros,
/// mas modelado para evoluir para outras espécies.
/// </summary>
public class Pet : EntityBase
{
    public const int NomeTamanhoMaximo = 80;
    public const int RacaTamanhoMaximo = 80;
    public const int MicrochipTamanhoMaximo = 20;
    public const int ObservacoesTamanhoMaximo = 500;
    public const decimal PesoMinimoKg = 0.1m;
    public const decimal PesoMaximoKg = 120m;

    private Pet() { }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Raça do cachorro (ex: Golden Retriever, SRD, Shih Tzu).</summary>
    public string Raca { get; private set; } = string.Empty;

    public DateTime DataNascimento { get; private set; }
    public Sexo Sexo { get; private set; }

    /// <summary>Peso atual em quilogramas. Importante para dosagem e score de risco.</summary>
    public decimal PesoKg { get; private set; }

    public PorteCachorro Porte { get; private set; }
    public string? Microchip { get; private set; }
    public bool Castrado { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public int TutorId { get; private set; }
    public Tutor? Tutor { get; private set; }

    public ICollection<Consulta> Consultas { get; private set; } = new List<Consulta>();
    public ICollection<Vacina> Vacinas { get; private set; } = new List<Vacina>();

    /// <summary>Idade atual em anos completos (não persistida).</summary>
    public int IdadeAnos => CalcularIdade(DateTime.Today);

    public static Pet Criar(
        string nome, string raca, DateTime dataNascimento, Sexo sexo,
        decimal pesoKg, PorteCachorro porte, string? microchip, bool castrado,
        string? observacoes, Tutor tutor)
    {
        DomainGuard.NaoNulo(tutor, "Tutor");

        var pet = new Pet
        {
            DataNascimento = DomainGuard.NaoFutura(dataNascimento, "Data de nascimento"),
            Sexo = DomainGuard.EnumDefinido(sexo, "Sexo"),
            Tutor = tutor,
            TutorId = tutor.Id,
            DataCadastro = DateTime.UtcNow
        };

        pet.Atualizar(nome, raca, pesoKg, porte, microchip, castrado, observacoes);
        return pet;
    }

    /// <summary>Atualiza dados clínicos e cadastrais. Sexo, nascimento e tutor são imutáveis.</summary>
    public void Atualizar(
        string nome, string raca, decimal pesoKg, PorteCachorro porte,
        string? microchip, bool castrado, string? observacoes)
    {
        Nome = DomainGuard.TextoObrigatorio(nome, "Nome do pet", NomeTamanhoMaximo);
        Raca = DomainGuard.TextoObrigatorio(raca, "Raça", RacaTamanhoMaximo);
        PesoKg = DomainGuard.Intervalo(pesoKg, PesoMinimoKg, PesoMaximoKg, "Peso (kg)");
        Porte = DomainGuard.EnumDefinido(porte, "Porte");
        Microchip = DomainGuard.TextoOpcional(microchip, "Microchip", MicrochipTamanhoMaximo);
        Castrado = castrado;
        Observacoes = DomainGuard.TextoOpcional(observacoes, "Observações", ObservacoesTamanhoMaximo);
    }

    /// <summary>Idade em anos completos numa data de referência (nunca negativa).</summary>
    public int CalcularIdade(DateTime referencia)
    {
        var idade = referencia.Year - DataNascimento.Year;
        if (DataNascimento.Date > referencia.Date.AddYears(-idade)) idade--;
        return Math.Max(idade, 0);
    }
}
