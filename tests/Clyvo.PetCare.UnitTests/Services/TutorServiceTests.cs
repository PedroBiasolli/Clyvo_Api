using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Services;

/// <summary>
/// Testes unitários do <see cref="TutorService"/> — camada de aplicação.
/// Usam EF InMemory (isolado por teste) no lugar do Oracle real.
/// Nomenclatura: MetodoTestado_Cenario_ResultadoEsperado.
/// </summary>
public class TutorServiceTests
{
    // ────────────────────────────── CriarAsync ──────────────────────────────

    [Fact]
    public async Task CriarAsync_DadosValidos_DevePersistirEDevolverDto()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new TutorService(db);
        var dto = new CriarTutorDto
        {
            Nome = "Mariana Silva",
            Cpf = "12345678901",
            Email = "mariana@email.com",
            Telefone = "11987654321",
            AceitaNotificacoes = true
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Mariana Silva", resultado.Nome);
        Assert.Equal("12345678901", resultado.Cpf);
        Assert.Single(db.Tutores);
    }

    [Fact]
    public async Task CriarAsync_CpfDuplicado_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Tutores.Add(EntityBuilder.NovoTutor(cpf: "11122233344"));
        await db.SaveChangesAsync();

        var service = new TutorService(db);
        var dto = new CriarTutorDto
        {
            Nome = "Outro tutor",
            Cpf = "11122233344",
            Email = "outro@email.com",
            Telefone = "11988887777"
        };

        // Act
        var acao = async () => await service.CriarAsync(dto);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(acao);
        Assert.Contains("CPF", ex.Message);
    }

    [Fact]
    public async Task CriarAsync_UfInformadoEmMinusculo_DevePersistirEmMaiusculo()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new TutorService(db);
        var dto = new CriarTutorDto
        {
            Nome = "Carlos",
            Cpf = "99988877766",
            Email = "carlos@email.com",
            Telefone = "11977776666",
            Uf = "sp"
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.Equal("SP", resultado.Uf);
    }

    // ────────────────────────────── BuscarPorId ──────────────────────────────

    [Fact]
    public async Task BuscarPorIdAsync_TutorExistente_DeveRetornarDto()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        // Act
        var resultado = await service.BuscarPorIdAsync(tutor.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(tutor.Id, resultado!.Id);
        Assert.Equal(tutor.Nome, resultado.Nome);
    }

    [Fact]
    public async Task BuscarPorIdAsync_TutorInexistente_DeveRetornarNull()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new TutorService(db);

        // Act
        var resultado = await service.BuscarPorIdAsync(999);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarPorCpfAsync_CpfExistente_DeveRetornarDto()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor(cpf: "55566677788");
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        // Act
        var resultado = await service.BuscarPorCpfAsync("55566677788");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("55566677788", resultado!.Cpf);
    }

    // ────────────────────────────── Listar ──────────────────────────────

    [Fact]
    public async Task ListarAsync_ComTresTutores_DeveRetornarTodosPaginados()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Tutores.AddRange(
            EntityBuilder.NovoTutor(nome: "Ana", cpf: "10000000001"),
            EntityBuilder.NovoTutor(nome: "Bruno", cpf: "10000000002"),
            EntityBuilder.NovoTutor(nome: "Carla", cpf: "10000000003"));
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        // Act
        var resultado = await service.ListarAsync(nome: null, cidade: null, pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(3, resultado.TotalRegistros);
        Assert.Equal(3, resultado.Itens.Count());
        Assert.Equal(1, resultado.Pagina);
    }

    [Fact]
    public async Task ListarAsync_FiltroPorNomeCaseInsensitive_DeveRetornarSomenteCorrespondentes()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        db.Tutores.AddRange(
            EntityBuilder.NovoTutor(nome: "Ana Silva", cpf: "20000000001"),
            EntityBuilder.NovoTutor(nome: "Bruno Souza", cpf: "20000000002"));
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        // Act
        var resultado = await service.ListarAsync(nome: "ana", cidade: null, pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal("Ana Silva", resultado.Itens.Single().Nome);
    }

    // ────────────────────────────── Atualizar ──────────────────────────────

    [Fact]
    public async Task AtualizarAsync_TutorExistente_DevePersistirNovosDados()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        var dto = new AtualizarTutorDto
        {
            Nome = "Nome Atualizado",
            Email = "novo@email.com",
            Telefone = "11900000000",
            AceitaNotificacoes = false
        };

        // Act
        var resultado = await service.AtualizarAsync(tutor.Id, dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Nome Atualizado", resultado!.Nome);
        Assert.Equal("novo@email.com", resultado.Email);
        Assert.False(resultado.AceitaNotificacoes);
    }

    [Fact]
    public async Task AtualizarAsync_TutorInexistente_DeveRetornarNull()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new TutorService(db);
        var dto = new AtualizarTutorDto
        {
            Nome = "Qualquer",
            Email = "x@y.com",
            Telefone = "11900000000"
        };

        // Act
        var resultado = await service.AtualizarAsync(999, dto);

        // Assert
        Assert.Null(resultado);
    }

    // ────────────────────────────── Remover ──────────────────────────────

    [Fact]
    public async Task RemoverAsync_TutorSemPets_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();
        var service = new TutorService(db);

        // Act
        var removido = await service.RemoverAsync(tutor.Id);

        // Assert
        Assert.True(removido);
        Assert.Empty(db.Tutores);
    }

    [Fact]
    public async Task RemoverAsync_TutorComPets_DeveLancarInvalidOperationException()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var tutor = EntityBuilder.NovoTutor();
        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        db.Pets.Add(EntityBuilder.NovoPet(tutor.Id));
        await db.SaveChangesAsync();

        var service = new TutorService(db);

        // Act
        var acao = async () => await service.RemoverAsync(tutor.Id);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(acao);
    }

    [Fact]
    public async Task RemoverAsync_TutorInexistente_DeveRetornarFalse()
    {
        // Arrange
        using var db = InMemoryDbContextFactory.Create();
        var service = new TutorService(db);

        // Act
        var removido = await service.RemoverAsync(9999);

        // Assert
        Assert.False(removido);
    }
}
