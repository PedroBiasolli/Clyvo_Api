using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Exceptions;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Casos de uso da carteira de vacinação com repositórios simulados (Moq). Padrão AAA.</summary>
public class VacinaServiceTests
{
    private readonly Mock<IVacinaRepository> _vacinas = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IVeterinarioRepository> _veterinarios = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly VacinaService _sut;

    public VacinaServiceTests()
        => _sut = new VacinaService(_vacinas.Object, _pets.Object, _veterinarios.Object,
            _unitOfWork.Object, _auditoria.Object, NullLogger<VacinaService>.Instance);

    private static CriarVacinaDto NovoDto(int? veterinarioId = null, DateTime? aplicacao = null) => new()
    {
        Nome = "Antirrábica",
        DataAplicacao = aplicacao ?? DateTime.Today,
        DataProximaDose = DateTime.Today.AddYears(1),
        Lote = "AR-01",
        Fabricante = "MSD",
        PetId = 1,
        VeterinarioId = veterinarioId
    };

    [Fact]
    public async Task CriarAsync_ComVeterinario_DeveRegistrarVinculandoProfissional()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));
        _veterinarios.Setup(r => r.ObterPorIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoVeterinario(id: 4));

        // Act
        var resultado = await _sut.CriarAsync(NovoDto(veterinarioId: 4));

        // Assert
        Assert.Equal(4, resultado.VeterinarioId);
        Assert.False(resultado.DoseAtrasada);
        _vacinas.Verify(r => r.AdicionarAsync(It.IsAny<Vacina>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_VeterinarioInformadoInexistente_DeveLancarRegraDeNegocio()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));
        _veterinarios.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Veterinario?)null);

        // Act
        var acao = () => _sut.CriarAsync(NovoDto(veterinarioId: 99));

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
    }

    [Fact]
    public async Task CriarAsync_AplicacaoFutura_DeveLancarDomainException()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));

        // Act
        var acao = () => _sut.CriarAsync(NovoDto(aplicacao: DateTime.Today.AddDays(1)));

        // Assert
        await Assert.ThrowsAsync<DomainException>(acao);
    }

    [Fact]
    public async Task BuscarDosesAtrasadasAsync_DeveMarcarDoseAtrasada()
    {
        // Arrange
        var atrasada = EntityBuilder.NovaVacina(dataAplicacao: DateTime.Today.AddYears(-1), dataProximaDose: DateTime.Today.AddDays(-3));
        _vacinas.Setup(r => r.ListarAtrasadasAsync(DateTime.Today, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { atrasada });

        // Act
        var resultado = await _sut.BuscarDosesAtrasadasAsync();

        // Assert
        var item = Assert.Single(resultado);
        Assert.True(item.DoseAtrasada);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public async Task BuscarProximasDosesAsync_JanelaInvalida_DeveLancarRegraDeNegocio(int dias)
    {
        // Act
        var acao = () => _sut.BuscarProximasDosesAsync(dias);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
    }

    [Fact]
    public async Task RemoverAsync_VacinaExistente_DeveRemoverEConfirmar()
    {
        // Arrange
        var vacina = EntityBuilder.NovaVacina(id: 6);
        _vacinas.Setup(r => r.ObterPorIdAsync(6, It.IsAny<CancellationToken>())).ReturnsAsync(vacina);

        // Act
        await _sut.RemoverAsync(6);

        // Assert
        _vacinas.Verify(r => r.Remover(vacina), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_Inexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _vacinas.Setup(r => r.ObterPorIdAsync(50, It.IsAny<CancellationToken>())).ReturnsAsync((Vacina?)null);
        var dto = new AtualizarVacinaDto { Nome = "V10", DataAplicacao = DateTime.Today };

        // Act
        var acao = () => _sut.AtualizarAsync(50, dto);

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }
}
