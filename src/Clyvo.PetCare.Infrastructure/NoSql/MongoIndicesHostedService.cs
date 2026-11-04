using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Clyvo.PetCare.Infrastructure.NoSql;

/// <summary>
/// Cria (de forma idempotente) os índices da coleção de auditoria na subida da API.
/// Roda em segundo plano e tolera o Mongo indisponível — o health check reporta o estado.
/// </summary>
public sealed class MongoIndicesHostedService : BackgroundService
{
    private readonly MongoDbContext _contexto;
    private readonly ILogger<MongoIndicesHostedService> _logger;

    public MongoIndicesHostedService(MongoDbContext contexto, ILogger<MongoIndicesHostedService> logger)
    {
        _contexto = contexto;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chaves = Builders<AuditoriaDocument>.IndexKeys;

        var indices = new[]
        {
            new CreateIndexModel<AuditoriaDocument>(
                chaves.Ascending(d => d.Entidade).Ascending(d => d.EntidadeId),
                new CreateIndexOptions { Name = "ix_entidade_entidadeId" }),
            new CreateIndexModel<AuditoriaDocument>(
                chaves.Descending(d => d.DataHoraUtc),
                new CreateIndexOptions { Name = "ix_dataHoraUtc" }),
            new CreateIndexModel<AuditoriaDocument>(
                chaves.Ascending(d => d.Usuario),
                new CreateIndexOptions { Name = "ix_usuario" })
        };

        try
        {
            await _contexto.Auditoria.Indexes.CreateManyAsync(indices, stoppingToken);
            _logger.LogInformation("Índices da coleção de auditoria garantidos no MongoDB");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Não foi possível criar os índices do MongoDB na inicialização");
        }
    }
}
