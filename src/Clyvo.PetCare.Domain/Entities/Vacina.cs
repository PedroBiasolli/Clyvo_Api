using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Exceptions;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Registro da carteira de vacinação digital do pet (V8/V10, Antirrábica,
/// Tosse dos canis, Giárdia, Gripe canina). Alimenta o motor preventivo
/// via <see cref="DataProximaDose"/>.
/// </summary>
public class Vacina : EntityBase
{
    public const int NomeTamanhoMaximo = 80;
    public const int LoteTamanhoMaximo = 50;
    public const int FabricanteTamanhoMaximo = 80;
    public const int ObservacoesTamanhoMaximo = 500;

    private Vacina() { }

    public string Nome { get; private set; } = string.Empty;
    public DateTime DataAplicacao { get; private set; }

    /// <summary>Data prevista para a próxima dose/reforço.</summary>
    public DateTime? DataProximaDose { get; private set; }

    public string? Lote { get; private set; }
    public string? Fabricante { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTime DataRegistro { get; private set; }

    public int PetId { get; private set; }
    public Pet? Pet { get; private set; }

    public int? VeterinarioId { get; private set; }
    public Veterinario? Veterinario { get; private set; }

    public static Vacina Registrar(
        string nome, DateTime dataAplicacao, DateTime? dataProximaDose,
        string? lote, string? fabricante, string? observacoes,
        Pet pet, Veterinario? veterinario)
    {
        DomainGuard.NaoNulo(pet, "Pet");

        var vacina = new Vacina
        {
            Pet = pet,
            PetId = pet.Id,
            DataRegistro = DateTime.UtcNow
        };

        vacina.Atualizar(nome, dataAplicacao, dataProximaDose, lote, fabricante, observacoes, veterinario);
        return vacina;
    }

    public void Atualizar(
        string nome, DateTime dataAplicacao, DateTime? dataProximaDose,
        string? lote, string? fabricante, string? observacoes, Veterinario? veterinario)
    {
        DomainGuard.NaoFutura(dataAplicacao, "Data de aplicação");

        if (dataProximaDose.HasValue && dataProximaDose.Value.Date <= dataAplicacao.Date)
            throw new DomainException("A próxima dose deve ser posterior à data de aplicação.");

        Nome = DomainGuard.TextoObrigatorio(nome, "Nome da vacina", NomeTamanhoMaximo);
        DataAplicacao = dataAplicacao;
        DataProximaDose = dataProximaDose;
        Lote = DomainGuard.TextoOpcional(lote, "Lote", LoteTamanhoMaximo);
        Fabricante = DomainGuard.TextoOpcional(fabricante, "Fabricante", FabricanteTamanhoMaximo);
        Observacoes = DomainGuard.TextoOpcional(observacoes, "Observações", ObservacoesTamanhoMaximo);
        Veterinario = veterinario;
        VeterinarioId = veterinario?.Id;
    }

    /// <summary>Indica se a próxima dose já venceu na data de referência.</summary>
    public bool EstaAtrasada(DateTime referencia)
        => DataProximaDose.HasValue && DataProximaDose.Value.Date < referencia.Date;
}
