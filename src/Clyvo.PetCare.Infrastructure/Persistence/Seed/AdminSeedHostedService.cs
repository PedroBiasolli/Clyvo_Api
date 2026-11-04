using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Infrastructure.Persistence.Seed;

/// <summary>
/// Executa o <see cref="UsuarioAdminSeeder"/> em segundo plano na subida da API.
/// Se o banco ainda não tiver as migrations aplicadas, apenas registra o aviso.
/// </summary>
public sealed class AdminSeedHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdminSeedHostedService> _logger;

    public AdminSeedHostedService(IServiceScopeFactory scopeFactory, ILogger<AdminSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var seeder = scope.ServiceProvider.GetRequiredService<UsuarioAdminSeeder>();
            await seeder.GarantirAdministradorAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex,
                "Não foi possível garantir o administrador padrão. Verifique se as migrations foram aplicadas (dotnet ef database update)");
        }
    }
}
