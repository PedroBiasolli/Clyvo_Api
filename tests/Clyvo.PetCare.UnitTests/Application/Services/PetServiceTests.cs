using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Exceptions;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Casos de uso de pets com repositórios simulados (Moq). Padrão AAA.</summary>
public class PetServiceTests
{
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<ITutorRepository> _tutores = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly PetService _sut;

    public PetServiceTests()
        => _sut = new PetService(_pets.Object, _tutores.Object, _unitOfWork.Object, _auditoria.Object, NullLogger<PetService>.Instance);

    private static CriarPetDto NovoDto(int tutorId = 1, string? microchip = "982000111222333", DateTime? nascimento = null) => new()
    {
        Nome = "Luna",
        Raca = "Border Collie",
        DataNascimento = nascimento ?? new DateTime(2021, 5, 20),
        Sexo = Sexo.Femea,
        PesoKg = 18m,
        Porte = PorteCachorro.Medio,
        Microchip = microchip,
        Castrado = true,
        TutorId = tutorId
    };

    [Fact]
    public async Task CriarAsync_TutorExistente_DeveCadastrarComNomeDoTutor()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 1, nome: "Carlos Mendes");
        _tutores.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);
        _pets.Setup(r => r.ExisteMicrochipAsync("982000111222333", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var resultado = await _sut.CriarAsync(NovoDto());

        // Assert
        Assert.Equal("Luna", resultado.Nome);
        Assert.Equal("Carlos Mendes", resultado.TutorNome);
        Assert.Equal(1, resultado.TutorId);
        _pets.Verify(r => r.AdicionarAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.RegistrarAsync("Pet", It.IsAny<object>(), AcaoAuditoria.Criacao,
            It.IsAny<object?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_TutorInexistente_DeveLancarRegraDeNegocio()
    {
        // Arrange
        _tutores.Setup(r => r.ObterPorIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((Tutor?)null);

        // Act
        var acao = () => _sut.CriarAsync(NovoDto(tutorId: 42));

        // Assert
        var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
        Assert.Contains("42", erro.Message);
    }

    [Fact]
    public async Task CriarAsync_MicrochipJaUtilizado_DeveLancarConflito()
    {
        // Arrange
        _tutores.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoTutor());
        _pets.Setup(r => r.ExisteMicrochipAsync("982000111222333", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var acao = () => _sut.CriarAsync(NovoDto());

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(acao);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_NascimentoFuturo_DeveLancarDomainException()
    {
        // Arrange
        _tutores.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoTutor());

        // Act
        var acao = () => _sut.CriarAsync(NovoDto(microchip: null, nascimento: DateTime.Today.AddDays(5)));

        // Assert
        await Assert.ThrowsAsync<DomainException>(acao);
    }

    [Fact]
    public async Task AtualizarAsync_MicrochipDeOutroPet_DeveLancarConflito()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));
        _pets.Setup(r => r.ExisteMicrochipAsync("CHIP-OUTRO", 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = new AtualizarPetDto { Nome = "Thor", Raca = "Golden", PesoKg = 30m, Porte = PorteCachorro.Grande, Microchip = "CHIP-OUTRO" };

        // Act
        var acao = () => _sut.AtualizarAsync(1, dto);

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(acao);
    }

    [Fact]
    public async Task BuscarPorTutorAsync_TutorInexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _tutores.Setup(r => r.ExisteAsync(77, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var acao = () => _sut.BuscarPorTutorAsync(77);

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }

    [Fact]
    public async Task BuscarPorTutorAsync_TutorExistente_DeveMapearOsPets()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 2);
        _tutores.Setup(r => r.ExisteAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _pets.Setup(r => r.ListarPorTutorAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[] { EntityBuilder.NovoPet(tutor, 1, "Bento"), EntityBuilder.NovoPet(tutor, 2, "Mel") });

        // Act
        var resultado = await _sut.BuscarPorTutorAsync(2);

        // Assert
        Assert.Equal(new[] { "Bento", "Mel" }, resultado.Select(p => p.Nome));
    }

    [Fact]
    public async Task RemoverAsync_PetComHistoricoClinico_DeveLancarRegraDeNegocio()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));
        _pets.Setup(r => r.PossuiHistoricoClinicoAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var acao = () => _sut.RemoverAsync(1);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
        _pets.Verify(r => r.Remover(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task BuscarPorMicrochipAsync_Inexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorMicrochipAsync("000", It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var acao = () => _sut.BuscarPorMicrochipAsync(" 000 ");

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }
}
