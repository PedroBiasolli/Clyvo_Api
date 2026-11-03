using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Application.DTOs.Consulta;

/// <summary>Filtros, ordenação e paginação da listagem de consultas.</summary>
public sealed class ConsultaQueryDto : PaginacaoQueryDto
{
    public TipoConsulta? Tipo { get; set; }
    public StatusConsulta? Status { get; set; }
    public int? PetId { get; set; }
    public int? VeterinarioId { get; set; }

    /// <summary>Consultas a partir desta data/hora (inclusive).</summary>
    public DateTime? DataInicio { get; set; }

    /// <summary>Consultas até esta data/hora (inclusive).</summary>
    public DateTime? DataFim { get; set; }

    /// <summary>Campo de ordenação: DataHora (padrão, decrescente), Tipo, Status ou ProximoRetorno.</summary>
    public ConsultaOrdenacao? OrdenarPor { get; set; }
}
