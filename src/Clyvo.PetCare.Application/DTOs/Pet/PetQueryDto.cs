using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Pet;

/// <summary>Filtros, ordenação e paginação da listagem de pets.</summary>
public sealed class PetQueryDto : PaginacaoQueryDto
{
    /// <summary>Busca parcial por nome.</summary>
    public string? Nome { get; set; }

    /// <summary>Busca parcial por raça.</summary>
    public string? Raca { get; set; }

    /// <summary>Porte exato (Mini, Pequeno, Medio, Grande, Gigante).</summary>
    public PorteCachorro? Porte { get; set; }

    /// <summary>Somente pets deste tutor.</summary>
    public int? TutorId { get; set; }

    /// <summary>Campo de ordenação: Nome (padrão), Raca, DataNascimento, PesoKg ou DataCadastro.</summary>
    public PetOrdenacao? OrdenarPor { get; set; }
}
