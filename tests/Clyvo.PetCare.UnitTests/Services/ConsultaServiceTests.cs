using Clyvo.PetCare.Api.Application.DTOs.Consulta;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.Api.Domain.Enums;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Services;

/// <summary>
/// Testes unitários do <see cref="ConsultaService"/>.
/// </summary>
public class ConsultaServiceTests
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
    public async Task CriarAsync_PetEVetValidos_DeveAgendarComStatusAgendada()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var service = new ConsultaService(db);
            var dto = new CriarConsultaDto
            {
                DataHora = DateTime.UtcNow.AddDays(2),
                Tipo = TipoConsulta.Preventiva,
                MotivoConsulta = "Check-up",
                PetId = petId,
                VeterinarioId = vetId
            };

            // Act
            var resultado = await service.CriarAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(StatusConsulta.Agendada, resultado!.Status);
            Assert.Equal(petId, resultado.PetId);
        }
    }

    [Fact]
    public async Task CriarAsync_VeterinarioInativo_DeveRetornarNull()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        var vet = EntityBuilder.NovoVeterinario(ativo: false);
        db.Veterinarios.Add(vet);
        await db.SaveChangesAsync();

        var pet = EntityBuilder.NovoPet(tutor.Id);
        db.Pets.Add(pet);
        await db.SaveChangesAsync();

        var service = new ConsultaService(db);
        var dto = new CriarConsultaDto
        {
            DataHora = DateTime.UtcNow.AddDays(1),
            Tipo = TipoConsulta.Rotina,
            PetId = pet.Id,
            VeterinarioId = vet.Id
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarRetornosProximosAsync_DentroDaJanela_DeveRetornarSomenteEssesRetornos()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var hoje = DateTime.Today;
            db.Consultas.AddRange(
                new Api.Domain.Entities.Consulta
                {
                    DataHora = hoje.AddDays(-30), Tipo = TipoConsulta.Rotina,
                    Status = StatusConsulta.Realizada, ProximoRetorno = hoje.AddDays(5),
                    PetId = petId, VeterinarioId = vetId, DataCriacao = DateTime.UtcNow
                },
                new Api.Domain.Entities.Consulta
                {
                    DataHora = hoje.AddDays(-15), Tipo = TipoConsulta.Rotina,
                    Status = StatusConsulta.Realizada, ProximoRetorno = hoje.AddDays(20),
                    PetId = petId, VeterinarioId = vetId, DataCriacao = DateTime.UtcNow
                });
            await db.SaveChangesAsync();

            var service = new ConsultaService(db);

            // Act
            var resultado = (await service.BuscarRetornosProximosAsync(diasAFrente: 7)).ToList();

            // Assert
            Assert.Single(resultado);
            Assert.Equal(hoje.AddDays(5), resultado.Single().ProximoRetorno);
        }
    }

    [Fact]
    public async Task RemoverAsync_ConsultaRealizada_DeveLancarInvalidOperationException()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var consulta = EntityBuilder.NovaConsulta(petId, vetId, status: StatusConsulta.Realizada);
            db.Consultas.Add(consulta);
            await db.SaveChangesAsync();

            var service = new ConsultaService(db);

            // Act
            var acao = async () => await service.RemoverAsync(consulta.Id);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(acao);
        }
    }

    [Fact]
    public async Task AtualizarAsync_MudarStatusEDiagnostico_DevePersistirAlteracoes()
    {
        // Arrange
        var (db, petId, vetId) = await SetupPetEVetAsync();
        using (db)
        {
            var consulta = EntityBuilder.NovaConsulta(petId, vetId);
            db.Consultas.Add(consulta);
            await db.SaveChangesAsync();

            var service = new ConsultaService(db);
            var dto = new AtualizarConsultaDto
            {
                DataHora = consulta.DataHora,
                Tipo = consulta.Tipo,
                Status = StatusConsulta.Realizada,
                Diagnostico = "Saudável, sem alterações",
                Prescricao = "Manter dieta atual",
                ProximoRetorno = DateTime.Today.AddMonths(6)
            };

            // Act
            var resultado = await service.AtualizarAsync(consulta.Id, dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(StatusConsulta.Realizada, resultado!.Status);
            Assert.Equal("Saudável, sem alterações", resultado.Diagnostico);
        }
    }
}
