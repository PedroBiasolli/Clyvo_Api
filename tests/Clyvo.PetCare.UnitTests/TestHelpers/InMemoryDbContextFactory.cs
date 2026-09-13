using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.UnitTests.TestHelpers;

/// <summary>
/// Fábrica de <see cref="AppDbContext"/> apoiada no provider InMemory do EF Core.
/// Cada chamada gera um banco isolado (nome único), garantindo que os testes
/// não se afetem entre si mesmo rodando em paralelo.
/// </summary>
internal static class InMemoryDbContextFactory
{
    public static AppDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
