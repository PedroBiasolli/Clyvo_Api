using System.Text.Json;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.DTOs.Auditoria;

/// <summary>Evento da trilha de auditoria armazenado no MongoDB.</summary>
public sealed class RegistroAuditoriaDto : RecursoDto
{
    public string Id { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public string EntidadeId { get; set; } = string.Empty;
    public AcaoAuditoria Acao { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime DataHoraUtc { get; set; }
    public string? CorrelationId { get; set; }

    /// <summary>Snapshot do recurso após a operação (documento JSON livre).</summary>
    public JsonElement? Dados { get; set; }
}

/// <summary>Filtros da trilha de auditoria (ordenada por data, mais recente primeiro por padrão).</summary>
public sealed class AuditoriaQueryDto : PaginacaoQueryDto
{
    /// <summary>Nome da entidade (Tutor, Pet, Veterinario, Consulta, Vacina, Usuario).</summary>
    public string? Entidade { get; set; }

    /// <summary>Identificador do registro auditado.</summary>
    public string? EntidadeId { get; set; }

    public AcaoAuditoria? Acao { get; set; }

    /// <summary>E-mail de quem executou a operação.</summary>
    public string? Usuario { get; set; }

    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}

/// <summary>Total de operações por entidade e ação (aggregation pipeline do MongoDB).</summary>
public sealed class ResumoAuditoriaDto
{
    public string Entidade { get; set; } = string.Empty;
    public AcaoAuditoria Acao { get; set; }
    public long Total { get; set; }
}
