using Clyvo.PetCare.Api.Application.DTOs.Vacina;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Services;

/// <summary>
/// Testes unitários do <see cref="VacinaService"/>.
/// </summary>
public class VacinaServiceTests
{
    private static async Task<(Api.Infrastructure.Persistence.AppDbContext db, int petId, int vetId)>
        SetupPetEVetAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        var vet = EntityBuilder.NovoVeterinario();
        db.Veterinarios.Add(vet);
        await db.SaveChangesAsync();

        var pet = EntityBuilder.NovoPet(tutor.Id);
        db.Pets.Add(pet);
        await db.SaveChangesAsync();

        return (db, pet.Id, vet.Id);
    }

    [Fact]
    public async Task CriarAsync_VacinaValida_DeveRegistrarNaCarteira()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var service = new VacinaService(db);
            var dto = new CriarVacinaDto
            {
                Nome = "V10",
                DataAplicacao = DateTime.Today.AddMonths(-1),
                DataProximaDose = DateTime.Today.AddMonths(11),
                Lote = "L-001",
                Fabricante = "Zoetis",
                PetId = petId,
                VeterinarioId = vetId
            };

            // Act
            var resultado = await service.CriarAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("V10", resultado!.Nome);
            Assert.False(resultado.DoseAtrasada);
        }
    }

    [Fact]
    public async Task CriarAsync_DataAplicacaoFutura_DeveLancarInvalidOperationException()
    {
        // Arrange
        var (db, petId, _) = await SetupPetEVetAsync();
        using (db)
        {
            var service = new VacinaService(db);
            var dto = new CriarVacinaDto
            {
                Nome = "V10",
                DataAplicacao = DateTime.Today.AddDays(1),
                PetId = petId
            };

            // Act
            var acao = async () => await service.CriarAsync(dto);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(acao);
        }
    }

    [Fact]
    public async Task CriarAsync_PetInexistente_DeveRetornarNull()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new VacinaService(db);
        var dto = new CriarVacinaDto
        {
            Nome = "V10",
            DataAplicacao = DateTime.Today.AddDays(-1),
            PetId = 9999
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarDosesAtrasadasAsync_ComDoseVencidaEDentroDoPrazo_DeveRetornarSomenteVencidas()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            db.Vacinas.AddRange(
                EntityBuilder.NovaVacina(petId, vetId, "V10",
                    dataAplicacao: DateTime.Today.AddYears(-2),
                    dataProximaDose: DateTime.Today.AddDays(-30)),  // atrasada
                EntityBuilder.NovaVacina(petId, vetId, "Antirrábica",
                    dataAplicacao: DateTime.Today.AddMonths(-2),
                    dataProximaDose: DateTime.Today.AddMonths(10))); // em dia
            await db.SaveChangesAsync();

            var service = new VacinaService(db);

            // Act
            var atrasadas = (await service.BuscarDosesAtrasadasAsync()).ToList();

            // Assert
            Assert.Single(atrasadas);
            Assert.Equal("V10", atrasadas.Single().Nome);
        }
    }

    [Fact]
    public async Task BuscarProximasDosesAsync_DentroDaJanela_DeveRetornarSomenteNoIntervalo()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            db.Vacinas.AddRange(
                EntityBuilder.NovaVacina(petId, vetId, "V10",
                    dataProximaDose: DateTime.Today.AddDays(10)),      // dentro
                EntityBuilder.NovaVacina(petId, vetId, "Antirrábica",
                    dataProximaDose: DateTime.Today.AddDays(100)));    // fora
            await db.SaveChangesAsync();

            var service = new VacinaService(db);

            // Act
            var proximas = (await service.BuscarProximasDosesAsync(diasAFrente: 30)).ToList();

            // Assert
            Assert.Single(proximas);
            Assert.Equal("V10", proximas.Single().Nome);
        }
    }

    [Fact]
    public async Task RemoverAsync_VacinaExistente_DeveRemoverERetornarTrue()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var vacina = EntityBuilder.NovaVacina(petId, vetId);
            db.Vacinas.Add(vacina);
            await db.SaveChangesAsync();

            var service = new VacinaService(db);

            // Act
            var removida = await service.RemoverAsync(vacina.Id);

            // Assert
            Assert.True(removida);
            Assert.Empty(db.Vacinas);
        }
    }
}
