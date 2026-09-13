using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clyvo.PetCare.IntegrationTests.Fixtures;

/// <summary>
/// Fábrica customizada de host de teste. Substitui o provider Oracle pelo
/// InMemory Database do EF Core, permitindo que os testes rodem sem depender
/// da rede da FIAP e de forma determinística.
/// </summary>
public class ClyvoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"clyvo-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove o DbContext registrado pelo Program.cs (Oracle) junto com
            // os descritores de options associados — se algum sobrar, o EF
            // reclama de dois providers configurados no mesmo contexto.
            RemoverDescritoresDe<DbContextOptions<AppDbContext>>(services);
            RemoverDescritoresDe<DbContextOptions>(services);
            RemoverDescritoresDe<AppDbContext>(services);

            // Registra o AppDbContext com InMemory. O nome do banco é fixo por
            // instância da fábrica, então todos os requests do mesmo host
            // enxergam o mesmo estado.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Atenção: nunca chamar services.BuildServiceProvider() aqui.
            // Isso cria um provider paralelo que resolve os mesmos singletons
            // (inclusive o Serilog), congelando o logger antes do provider real
            // subir e quebrando o host com "The logger is already frozen".
            // A criação do schema acontece sob demanda em ResetDatabase().
        });
    }

    private static void RemoverDescritoresDe<T>(IServiceCollection services)
    {
        var toRemove = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in toRemove) services.Remove(d);
    }

    /// <summary>
    /// Garante um banco limpo para o teste que está começando. Como os testes
    /// compartilham o host via Collection Fixture, cada classe chama este
    /// método no construtor para não herdar dados do teste anterior.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
