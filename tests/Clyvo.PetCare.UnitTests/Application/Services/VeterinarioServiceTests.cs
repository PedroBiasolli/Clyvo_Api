using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Casos de uso de veterinários com repositórios simulados (Moq). Padrão AAA.</summary>
public class VeterinarioServiceTests
{
    private readonly Mock<IVeterinarioRepository> _veterinarios = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly VeterinarioService _sut;

    public VeterinarioServiceTests()
        => _sut = new VeterinarioService(_veterinarios.Object, _unitOfWork.Object, _auditoria.Object, NullLogger<VeterinarioService>.Instance);

    [Fact]
    public async Task CriarAsync_CrmvNovo_DeveCadastrarComCrmvNormalizado()
    {
        // Arrange
        _veterinarios.Setup(r => r.ExisteCrmvAsync("CRMV-SP 777", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var dto = new CriarVeterinarioDto { Nome = "Dr. Paulo", Crmv = "crmv-sp 777", Email = "paulo@clyvo.com.br" };

        // Act
        var resultado = await _sut.CriarAsync(dto);

        // Assert
        Assert.Equal("CRMV-SP 777", resultado.Crmv);
        Assert.True(resultado.Ativo);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_CrmvDuplicado_DeveLancarConflito()
    {
        // Arrange
        _veterinarios.Setup(r => r.ExisteCrmvAsync("CRMV-SP 777", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = new CriarVeterinarioDto { Nome = "Dr. Paulo", Crmv = "CRMV-SP 777", Email = "paulo@clyvo.com.br" };

        // Act
        var acao = () => _sut.CriarAsync(dto);

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(acao);
    }

    [Fact]
    public async Task AtualizarAsync_Inativando_DeveRefletirNoDto()
    {
        // Arrange
        _veterinarios.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoVeterinario());
        var dto = new AtualizarVeterinarioDto { Nome = "Dra. Ana Costa", Email = "ana@clyvo.com.br", Ativo = false };

        // Act
        var resultado = await _sut.AtualizarAsync(1, dto);

        // Assert
        Assert.False(resultado.Ativo);
    }

    [Fact]
    public async Task RemoverAsync_ComConsultas_DeveLancarRegraDeNegocio()
    {
        // Arrange
        _veterinarios.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoVeterinario());
        _veterinarios.Setup(r => r.PossuiConsultasAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var acao = () => _sut.RemoverAsync(1);

        // Assert
        var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
        Assert.Contains("inativo", erro.Message);
    }

    [Fact]
    public async Task BuscarPorCrmvAsync_DeveConsultarEmCaixaAlta()
    {
        // Arrange
        _veterinarios.Setup(r => r.ObterPorCrmvAsync("CRMV-SP 12345", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(EntityBuilder.NovoVeterinario());

        // Act
        var resultado = await _sut.BuscarPorCrmvAsync(" crmv-sp 12345 ");

        // Assert
        Assert.Equal("CRMV-SP 12345", resultado.Crmv);
    }

    [Fact]
    public async Task BuscarPorIdAsync_Inexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _veterinarios.Setup(r => r.ObterPorIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync((Veterinario?)null);

        // Act
        var acao = () => _sut.BuscarPorIdAsync(8);

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }
}
