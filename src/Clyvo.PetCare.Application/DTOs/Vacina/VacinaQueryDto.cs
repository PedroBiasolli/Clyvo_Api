using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Vacina;

/// <summary>Filtros, ordenação e paginação da listagem de vacinas.</summary>
public sealed class VacinaQueryDto : PaginacaoQueryDto
{
    /// <summary>Busca parcial pelo nome da vacina.</summary>
    public string? Nome { get; set; }

    /// <summary>Somente vacinas deste pet.</summary>
    public int? PetId { get; set; }

    /// <summary>Campo de ordenação: DataAplicacao (padrão, decrescente), DataProximaDose ou Nome.</summary>
    public VacinaOrdenacao? OrdenarPor { get; set; }
}
