using Clyvo.PetCare.Infrastructure.NoSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Clyvo.PetCare.Infrastructure.HealthChecks;

/// <summary>Verifica a disponibilidade do MongoDB com o comando <c>ping</c>.</summary>
public sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _contexto;

    public MongoDbHealthCheck(MongoDbContext contexto) => _contexto = contexto;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var ping = new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1));
            await _contexto.Database.RunCommandAsync(ping, cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB respondendo ao ping.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "MongoDB indisponível.", ex);
        }
    }
}
