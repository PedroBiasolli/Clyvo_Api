using Clyvo.PetCare.Api.Application.DTOs.Veterinario;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Services;

/// <summary>
/// Testes unitários do <see cref="VeterinarioService"/>.
/// </summary>
public class VeterinarioServiceTests
{
    [Fact]
    public async Task CriarAsync_CrmvNovo_DevePersistirVeterinarioAtivo()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new VeterinarioService(db);
        var dto = new CriarVeterinarioDto
        {
            Nome = "Dra. Ana Costa",
            Crmv = "SP12345",
            Especialidade = "Clínica Geral",
            Email = "ana@clyvo.com",
            Telefone = "11999998888"
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Ativo);
        Assert.Equal("SP12345", resultado.Crmv);
    }

    [Fact]
    public async Task CriarAsync_CrmvDuplicado_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Veterinarios.Add(EntityBuilder.NovoVeterinario(crmv: "SP99999"));
        await db.SaveChangesAsync();

        var service = new VeterinarioService(db);
        var dto = new CriarVeterinarioDto
        {
            Nome = "Outro vet",
            Crmv = "SP99999",
            Email = "outro@clyvo.com"
        };

        // Act
        var acao = async () => await service.CriarAsync(dto);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(acao);
        Assert.Contains("CRMV", ex.Message);
    }

    [Fact]
    public async Task BuscarPorCrmvAsync_CrmvExistente_DeveRetornarVeterinario()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Veterinarios.Add(EntityBuilder.NovoVeterinario(crmv: "RJ54321"));
        await db.SaveChangesAsync();
        var service = new VeterinarioService(db);

        // Act
        var resultado = await service.BuscarPorCrmvAsync("RJ54321");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("RJ54321", resultado!.Crmv);
    }

    [Fact]
    public async Task ListarAsync_FiltroPorAtivoFalse_DeveRetornarSomenteInativos()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Veterinarios.AddRange(
            EntityBuilder.NovoVeterinario(nome: "Ativo1", crmv: "SP1", ativo: true),
            EntityBuilder.NovoVeterinario(nome: "Inativo1", crmv: "SP2", ativo: false),
            EntityBuilder.NovoVeterinario(nome: "Inativo2", crmv: "SP3", ativo: false));
        await db.SaveChangesAsync();
        var service = new VeterinarioService(db);

        // Act
        var resultado = await service.ListarAsync(especialidade: null, ativo: false,
            pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(2, resultado.TotalRegistros);
        Assert.All(resultado.Itens, v => Assert.False(v.Ativo));
    }

    [Fact]
    public async Task AtualizarAsync_MudarAtivoParaFalse_DevePersistirInativacao()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var vet = EntityBuilder.NovoVeterinario(ativo: true);
        db.Veterinarios.Add(vet);
        await db.SaveChangesAsync();
        var service = new VeterinarioService(db);

        var dto = new AtualizarVeterinarioDto
        {
            Nome = vet.Nome,
            Email = vet.Email,
            Especialidade = vet.Especialidade,
            Telefone = vet.Telefone,
            Ativo = false
        };

        // Act
        var resultado = await service.AtualizarAsync(vet.Id, dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado!.Ativo);
    }

    [Fact]
    public async Task RemoverAsync_VeterinarioComConsultas_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        var vet = EntityBuilder.NovoVeterinario();
        db.Veterinarios.Add(vet);
        await db.SaveChangesAsync();

        var pet = EntityBuilder.NovoPet(tutor.Id);
        db.Pets.Add(pet);
        await db.SaveChangesAsync();

        db.Consultas.Add(EntityBuilder.NovaConsulta(pet.Id, vet.Id));
        await db.SaveChangesAsync();

        var service = new VeterinarioService(db);

        // Act
        var acao = async () => await service.RemoverAsync(vet.Id);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(acao);
    }
}
