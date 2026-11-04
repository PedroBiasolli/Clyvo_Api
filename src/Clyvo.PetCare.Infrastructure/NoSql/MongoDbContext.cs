using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Clyvo.PetCare.Infrastructure.NoSql;

/// <summary>Ponto único de acesso ao banco e às coleções do MongoDB.</summary>
public sealed class MongoDbContext
{
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        _settings = options.Value;
        Database = client.GetDatabase(_settings.Database);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<AuditoriaDocument> Auditoria
        => Database.GetCollection<AuditoriaDocument>(_settings.ColecaoAuditoria);
}
