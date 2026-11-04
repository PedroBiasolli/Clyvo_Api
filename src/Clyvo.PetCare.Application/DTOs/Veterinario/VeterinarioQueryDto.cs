using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Veterinario;

/// <summary>Filtros, ordenação e paginação da listagem de veterinários.</summary>
public sealed class VeterinarioQueryDto : PaginacaoQueryDto
{
    /// <summary>Busca parcial por nome.</summary>
    public string? Nome { get; set; }

    /// <summary>Busca parcial por especialidade.</summary>
    public string? Especialidade { get; set; }

    /// <summary>Somente ativos (true) ou inativos (false).</summary>
    public bool? Ativo { get; set; }

    /// <summary>Campo de ordenação: Nome (padrão), Especialidade ou DataCadastro.</summary>
    public VeterinarioOrdenacao? OrdenarPor { get; set; }
}
