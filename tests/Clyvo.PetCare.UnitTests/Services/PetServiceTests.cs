using Clyvo.PetCare.Api.Application.DTOs.Pet;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.Api.Domain.Enums;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Services;

/// <summary>
/// Testes unitários do <see cref="PetService"/>.
/// </summary>
public class PetServiceTests
{
    [Fact]
    public async Task CriarAsync_TutorExistente_DevePersistirPet()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        var service = new PetService(db);
        var dto = new CriarPetDto
        {
            Nome = "Thor",
            Raca = "Golden Retriever",
            DataNascimento = new DateTime(2022, 3, 15),
            Sexo = Sexo.Macho,
            PesoKg = 28.5m,
            Porte = PorteCachorro.Grande,
            Castrado = true,
            TutorId = tutor.Id
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Thor", resultado!.Nome);
        Assert.Equal(tutor.Id, resultado.TutorId);
        Assert.Single(db.Pets);
    }

    [Fact]
    public async Task CriarAsync_TutorInexistente_DeveRetornarNull()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new PetService(db);
        var dto = new CriarPetDto
        {
            Nome = "Fantasma",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(-2),
            Sexo = Sexo.Femea,
            PesoKg = 10m,
            Porte = PorteCachorro.Pequeno,
            TutorId = 9999
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task CriarAsync_DataNascimentoFutura_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        var service = new PetService(db);
        var dto = new CriarPetDto
        {
            Nome = "Futuro",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddDays(1),
            Sexo = Sexo.Macho,
            PesoKg = 5m,
            Porte = PorteCachorro.Mini,
            TutorId = tutor.Id
        };

        // Act
        var acao = async () => await service.CriarAsync(dto);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(acao);
        Assert.Contains("Data de nascimento", ex.Message);
    }

    [Fact]
    public async Task BuscarPorMicrochipAsync_MicrochipExistente_DeveRetornarPet()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        db.Pets.Add(EntityBuilder.NovoPet(tutor.Id, microchip: "982000900000001"));
        await db.SaveChangesAsync();

        var service = new PetService(db);

        // Act
        var resultado = await service.BuscarPorMicrochipAsync("982000900000001");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("982000900000001", resultado!.Microchip);
    }

    [Fact]
    public async Task BuscarPorTutorAsync_TutorComDoisPets_DeveRetornarAmbos()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        db.Pets.AddRange(
            EntityBuilder.NovoPet(tutor.Id, nome: "Thor", microchip: "111"),
            EntityBuilder.NovoPet(tutor.Id, nome: "Luna", microchip: "222"));
        await db.SaveChangesAsync();

        var service = new PetService(db);

        // Act
        var resultado = (await service.BuscarPorTutorAsync(tutor.Id)).ToList();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, p => p.Nome == "Thor");
        Assert.Contains(resultado, p => p.Nome == "Luna");
    }

    [Fact]
    public async Task RemoverAsync_PetComVacinas_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        var pet = EntityBuilder.NovoPet(tutor.Id);
        db.Pets.Add(pet);
        await db.SaveChangesAsync();

        db.Vacinas.Add(EntityBuilder.NovaVacina(pet.Id));
        await db.SaveChangesAsync();

        var service = new PetService(db);

        // Act
        var acao = async () => await service.RemoverAsync(pet.Id);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(acao);
    }

    [Fact]
    public async Task ListarAsync_FiltroPorPorte_DeveRetornarSomentePortesSelecionados()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        db.Pets.AddRange(
            EntityBuilder.NovoPet(tutor.Id, nome: "Rex", porte: PorteCachorro.Grande, microchip: "A"),
            EntityBuilder.NovoPet(tutor.Id, nome: "Mel", porte: PorteCachorro.Pequeno, microchip: "B"),
            EntityBuilder.NovoPet(tutor.Id, nome: "Bidu", porte: PorteCachorro.Grande, microchip: "C"));
        await db.SaveChangesAsync();

        var service = new PetService(db);

        // Act
        var resultado = await service.ListarAsync(nome: null, raca: null,
            porte: PorteCachorro.Grande, pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(2, resultado.TotalRegistros);
        Assert.All(resultado.Itens, p => Assert.Equal(PorteCachorro.Grande, p.Porte));
    }
}
