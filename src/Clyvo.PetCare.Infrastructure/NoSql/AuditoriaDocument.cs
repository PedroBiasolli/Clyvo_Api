using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;

namespace Clyvo.PetCare.Infrastructure.NoSql;

/// <summary>
/// Modelo de persistência do evento de auditoria no MongoDB. Os atributos BSON
/// ficam aqui (e não no domínio) para manter o domínio agnóstico de banco.
/// O campo <see cref="Dados"/> é um subdocumento livre — o snapshot do recurso.
/// </summary>
public sealed class AuditoriaDocument
{
    private static readonly JsonWriterSettings JsonRelaxado = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("entidade")]
    public string Entidade { get; set; } = string.Empty;

    [BsonElement("entidadeId")]
    public string EntidadeId { get; set; } = string.Empty;

    [BsonElement("acao")]
    [BsonRepresentation(BsonType.String)]
    public AcaoAuditoria Acao { get; set; }

    [BsonElement("usuario")]
    public string Usuario { get; set; } = string.Empty;

    [BsonElement("dataHoraUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataHoraUtc { get; set; }

    [BsonElement("correlationId")]
    [BsonIgnoreIfNull]
    public string? CorrelationId { get; set; }

    [BsonElement("dados")]
    [BsonIgnoreIfNull]
    public BsonDocument? Dados { get; set; }

    public static AuditoriaDocument De(RegistroAuditoria registro) => new()
    {
        Id = registro.Id,
        Entidade = registro.Entidade,
        EntidadeId = registro.EntidadeId,
        Acao = registro.Acao,
        Usuario = registro.Usuario,
        DataHoraUtc = registro.DataHoraUtc,
        CorrelationId = registro.CorrelationId,
        Dados = string.IsNullOrWhiteSpace(registro.DadosJson) ? null : BsonDocument.Parse(registro.DadosJson)
    };

    public RegistroAuditoria ParaDominio() => RegistroAuditoria.Reconstituir(
        Id ?? string.Empty, Entidade, EntidadeId, Acao, Usuario, DataHoraUtc, CorrelationId,
        Dados?.ToJson(JsonRelaxado));
}
