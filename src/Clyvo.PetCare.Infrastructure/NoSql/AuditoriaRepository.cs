using System.Text.RegularExpressions;
using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Clyvo.PetCare.Infrastructure.NoSql;

/// <summary>
/// Repositório da trilha de auditoria no MongoDB: inserção append-only,
/// consulta filtrada/paginada e resumo via aggregation pipeline.
/// </summary>
internal sealed class AuditoriaRepository : IAuditoriaRepository
{
    private static readonly FilterDefinitionBuilder<AuditoriaDocument> Filtro = Builders<AuditoriaDocument>.Filter;
    private static readonly SortDefinitionBuilder<AuditoriaDocument> Ordem = Builders<AuditoriaDocument>.Sort;

    private readonly IMongoCollection<AuditoriaDocument> _colecao;

    public AuditoriaRepository(MongoDbContext contexto) => _colecao = contexto.Auditoria;

    public Task InserirAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        => _colecao.InsertOneAsync(AuditoriaDocument.De(registro), cancellationToken: cancellationToken);

    public async Task<ResultadoPaginado<RegistroAuditoria>> ListarAsync(AuditoriaFiltro filtro, CancellationToken cancellationToken = default)
    {
        var criterio = MontarCriterio(filtro);

        var total = await _colecao.CountDocumentsAsync(criterio, cancellationToken: cancellationToken);

        var ordenacao = filtro.Direcao == DirecaoOrdenacao.Asc
            ? Ordem.Ascending(d => d.DataHoraUtc)
            : Ordem.Descending(d => d.DataHoraUtc);

        var documentos = await _colecao.Find(criterio)
            .Sort(ordenacao)
            .Skip(filtro.Deslocamento)
            .Limit(filtro.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<RegistroAuditoria>(
            documentos.Select(d => d.ParaDominio()).ToList(),
            (int)total);
    }

    public async Task<RegistroAuditoria?> ObterPorIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var documento = await _colecao.Find(d => d.Id == id).FirstOrDefaultAsync(cancellationToken);
        return documento?.ParaDominio();
    }

    public async Task<IReadOnlyList<ResumoAuditoria>> ResumirPorEntidadeEAcaoAsync(DateTime? desde, CancellationToken cancellationToken = default)
    {
        var estagios = new List<BsonDocument>();

        if (desde.HasValue)
        {
            estagios.Add(new BsonDocument("$match",
                new BsonDocument("dataHoraUtc", new BsonDocument("$gte", ComoUtc(desde.Value)))));
        }

        estagios.Add(new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument { { "entidade", "$entidade" }, { "acao", "$acao" } } },
            { "total", new BsonDocument("$sum", 1) }
        }));

        estagios.Add(new BsonDocument("$sort", new BsonDocument { { "_id.entidade", 1 }, { "_id.acao", 1 } }));

        PipelineDefinition<AuditoriaDocument, BsonDocument> pipeline = estagios;
        using var cursor = await _colecao.AggregateAsync(pipeline, cancellationToken: cancellationToken);
        var linhas = await cursor.ToListAsync(cancellationToken);

        return linhas
            .Select(ParaResumo)
            .OfType<ResumoAuditoria>()
            .ToList();
    }

    private static ResumoAuditoria? ParaResumo(BsonDocument linha)
    {
        var chave = linha["_id"].AsBsonDocument;

        return Enum.TryParse<AcaoAuditoria>(chave["acao"].AsString, out var acao)
            ? new ResumoAuditoria(chave["entidade"].AsString, acao, linha["total"].ToInt64())
            : null;
    }

    private static FilterDefinition<AuditoriaDocument> MontarCriterio(AuditoriaFiltro filtro)
    {
        var criterios = new List<FilterDefinition<AuditoriaDocument>>();

        if (!string.IsNullOrWhiteSpace(filtro.Entidade))
            criterios.Add(Filtro.Regex(d => d.Entidade, IgualSemCaixa(filtro.Entidade)));

        if (!string.IsNullOrWhiteSpace(filtro.EntidadeId))
            criterios.Add(Filtro.Eq(d => d.EntidadeId, filtro.EntidadeId.Trim()));

        if (filtro.Acao.HasValue)
            criterios.Add(Filtro.Eq(d => d.Acao, filtro.Acao.Value));

        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
            criterios.Add(Filtro.Regex(d => d.Usuario, ContemSemCaixa(filtro.Usuario)));

        if (filtro.DataInicio.HasValue)
            criterios.Add(Filtro.Gte(d => d.DataHoraUtc, ComoUtc(filtro.DataInicio.Value)));

        if (filtro.DataFim.HasValue)
            criterios.Add(Filtro.Lte(d => d.DataHoraUtc, ComoUtc(filtro.DataFim.Value)));

        return criterios.Count == 0 ? Filtro.Empty : Filtro.And(criterios);
    }

    private static BsonRegularExpression IgualSemCaixa(string valor)
        => new($"^{Regex.Escape(valor.Trim())}$", "i");

    private static BsonRegularExpression ContemSemCaixa(string valor)
        => new(Regex.Escape(valor.Trim()), "i");

    /// <summary>Datas de filtro são interpretadas como UTC (padrão de armazenamento da auditoria).</summary>
    private static DateTime ComoUtc(DateTime data)
        => data.Kind == DateTimeKind.Utc ? data : DateTime.SpecifyKind(data, DateTimeKind.Utc);
}
