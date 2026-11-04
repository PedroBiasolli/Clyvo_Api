using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Evento da trilha de auditoria. Diferente das demais entidades, é persistido
/// no MongoDB (documento append-only, com o snapshot dos dados em JSON livre),
/// por isso não herda de <see cref="EntityBase"/> e usa identificador textual.
/// </summary>
public sealed class RegistroAuditoria
{
    private RegistroAuditoria() { }

    public string? Id { get; private set; }
    public string Entidade { get; private set; } = string.Empty;
    public string EntidadeId { get; private set; } = string.Empty;
    public AcaoAuditoria Acao { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime DataHoraUtc { get; private set; }
    public string? CorrelationId { get; private set; }

    /// <summary>Snapshot do recurso após a operação, serializado em JSON.</summary>
    public string? DadosJson { get; private set; }

    public static RegistroAuditoria Criar(
        string entidade, string entidadeId, AcaoAuditoria acao,
        string usuario, string? correlationId, string? dadosJson) => new()
    {
        Entidade = DomainGuard.TextoObrigatorio(entidade, "Entidade auditada", 60),
        EntidadeId = DomainGuard.TextoObrigatorio(entidadeId, "Identificador auditado", 60),
        Acao = DomainGuard.EnumDefinido(acao, "Ação auditada"),
        Usuario = string.IsNullOrWhiteSpace(usuario) ? "anonimo" : usuario.Trim(),
        DataHoraUtc = DateTime.UtcNow,
        CorrelationId = correlationId,
        DadosJson = dadosJson
    };

    /// <summary>Reidrata um registro já persistido (uso exclusivo dos repositórios).</summary>
    public static RegistroAuditoria Reconstituir(
        string id, string entidade, string entidadeId, AcaoAuditoria acao,
        string usuario, DateTime dataHoraUtc, string? correlationId, string? dadosJson) => new()
    {
        Id = id,
        Entidade = entidade,
        EntidadeId = entidadeId,
        Acao = acao,
        Usuario = usuario,
        DataHoraUtc = dataHoraUtc,
        CorrelationId = correlationId,
        DadosJson = dadosJson
    };
}
