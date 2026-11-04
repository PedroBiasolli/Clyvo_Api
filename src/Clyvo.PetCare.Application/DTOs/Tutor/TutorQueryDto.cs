using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Tutor;

/// <summary>Filtros, ordenação e paginação da listagem de tutores.</summary>
public sealed class TutorQueryDto : PaginacaoQueryDto
{
    /// <summary>Busca parcial por nome (sem diferenciar maiúsculas).</summary>
    public string? Nome { get; set; }

    /// <summary>Cidade exata (sem diferenciar maiúsculas).</summary>
    public string? Cidade { get; set; }

    /// <summary>Campo de ordenação: Nome (padrão), Cidade ou DataCadastro.</summary>
    public TutorOrdenacao? OrdenarPor { get; set; }
}
