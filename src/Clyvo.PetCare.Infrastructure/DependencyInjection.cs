using System.Text;
using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.Infrastructure.HealthChecks;
using Clyvo.PetCare.Infrastructure.NoSql;
using Clyvo.PetCare.Infrastructure.Persistence;
using Clyvo.PetCare.Infrastructure.Persistence.Repositories;
using Clyvo.PetCare.Infrastructure.Persistence.Seed;
using Clyvo.PetCare.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Clyvo.PetCare.Infrastructure;

/// <summary>
/// Composição da infraestrutura: cada contrato do Domínio/Aplicação recebe aqui
/// sua implementação concreta (Dependency Inversion).
/// </summary>
public static class DependencyInjection
{
    public const string ConnectionStringOracle = "OracleFiap";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services
            .AddPersistenciaRelacional(configuration, environment)
            .AddPersistenciaNoSql(configuration)
            .AddSeguranca(configuration);

        return services;
    }

    /// <summary>Health checks das dependências externas (Oracle crítico; MongoDB degradável).</summary>
    public static IHealthChecksBuilder AddInfrastructureHealthChecks(this IHealthChecksBuilder builder)
        => builder
            .AddDbContextCheck<AppDbContext>(
                name: "oracle-database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "db", "oracle" })
            .AddCheck<MongoDbHealthCheck>(
                name: "mongodb",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "nosql", "mongodb" });

    private static IServiceCollection AddPersistenciaRelacional(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringOracle)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringOracle}' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseOracle(connectionString, oracle =>
                    oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19))
                .EnableDetailedErrors(environment.IsDevelopment()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IVeterinarioRepository, VeterinarioRepository>();
        services.AddScoped<IConsultaRepository, ConsultaRepository>();
        services.AddScoped<IVacinaRepository, VacinaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.Configure<AdminSeedSettings>(configuration.GetSection(AdminSeedSettings.Secao));
        services.AddScoped<UsuarioAdminSeeder>();
        services.AddHostedService<AdminSeedHostedService>();

        return services;
    }

    private static IServiceCollection AddPersistenciaNoSql(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.Secao));

        // MongoClient é thread-safe e mantém o pool de conexões: uma instância por aplicação.
        services.AddSingleton<IMongoClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var timeout = TimeSpan.FromSeconds(settings.TimeoutSegundos);

            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ServerSelectionTimeout = timeout;
            clientSettings.ConnectTimeout = timeout;

            return new MongoClient(clientSettings);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddHostedService<MongoIndicesHostedService>();

        return services;
    }

    private static IServiceCollection AddSeguranca(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.Secao))
            .Validate(
                jwt => Encoding.UTF8.GetByteCount(jwt.ChaveSecreta) >= JwtSettings.TamanhoMinimoChaveBytes,
                $"Jwt:ChaveSecreta deve ter pelo menos {JwtSettings.TamanhoMinimoChaveBytes} bytes.")
            .Validate(jwt => jwt.ExpiracaoMinutos > 0, "Jwt:ExpiracaoMinutos deve ser positivo.")
            .ValidateOnStart();

        services.AddSingleton<IGeradorToken, JwtGeradorToken>();
        services.AddSingleton<IHashSenha, HashSenhaPbkdf2>();

        return services;
    }
}
