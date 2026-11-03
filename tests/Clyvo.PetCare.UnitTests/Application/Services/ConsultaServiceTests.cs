using Clyvo.PetCare.Application.DTOs.Consulta;
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

/// <summary>Casos de uso de consultas com repositórios simulados (Moq). Padrão AAA.</summary>
public class ConsultaServiceTests
{
    private readonly Mock<IConsultaRepository> _consultas = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IVeterinarioRepository> _veterinarios = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly ConsultaService _sut;

    public ConsultaServiceTests()
        => _sut = new ConsultaService(_consultas.Object, _pets.Object, _veterinarios.Object,
            _unitOfWork.Object, _auditoria.Object, NullLogger<ConsultaService>.Instance);

    private static CriarConsultaDto NovoDto() => new()
    {
        DataHora = DateTime.Today.AddDays(3).AddHours(9),
        Tipo = TipoConsulta.Preventiva,
        MotivoConsulta = "Check-up",
        PetId = 1,
        VeterinarioId = 2
    };

    [Fact]
    public async Task CriarAsync_PetEVeterinarioValidos_DeveAgendarComStatusAgendada()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1, nome: "Thor"));
        _veterinarios.Setup(r => r.ObterPorIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoVeterinario(id: 2));

        // Act
        var resultado = await _sut.CriarAsync(NovoDto());

        // Assert
        Assert.Equal(StatusConsulta.Agendada, resultado.Status);
        Assert.Equal("Thor", resultado.PetNome);
        Assert.Equal("Dra. Ana Costa", resultado.VeterinarioNome);
        _consultas.Verify(r => r.AdicionarAsync(It.IsAny<Consulta>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_VeterinarioInativo_DeveLancarDomainException()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoPet(id: 1));
        _veterinarios.Setup(r => r.ObterPorIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(EntityBuilder.NovoVeterinario(id: 2, ativo: false));

        // Act
        var acao = () => _sut.CriarAsync(NovoDto());

        // Assert
        await Assert.ThrowsAsync<DomainException>(acao);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_PetInexistente_DeveLancarRegraDeNegocio()
    {
        // Arrange
        _pets.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var acao = () => _sut.CriarAsync(NovoDto());

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
    }

    [Fact]
    public async Task AtualizarAsync_RealizadaComDiagnostico_DeveConfirmarEAuditar()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(id: 10);
        _consultas.Setup(r => r.ObterPorIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        var dto = new AtualizarConsultaDto
        {
            DataHora = consulta.DataHora,
            Tipo = consulta.Tipo,
            Status = StatusConsulta.Realizada,
            Diagnostico = "Otite leve",
            Prescricao = "Limpeza auricular",
            ProximoRetorno = consulta.DataHora.AddDays(15),
            ValorCobrado = 250m
        };

        // Act
        var resultado = await _sut.AtualizarAsync(10, dto);

        // Assert
        Assert.Equal(StatusConsulta.Realizada, resultado.Status);
        Assert.Equal("Otite leve", resultado.Diagnostico);
        _auditoria.Verify(a => a.RegistrarAsync("Consulta", 10, AcaoAuditoria.Atualizacao,
            It.IsAny<object?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_ConsultaRealizada_DeveLancarDomainExceptionSemRemover()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(id: 3);
        consulta.Atualizar(consulta.DataHora, consulta.Tipo, StatusConsulta.Realizada, null, "Ok", null, null, null, null);
        _consultas.Setup(r => r.ObterPorIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        // Act
        var acao = () => _sut.RemoverAsync(3);

        // Assert
        await Assert.ThrowsAsync<DomainException>(acao);
        _consultas.Verify(r => r.Remover(It.IsAny<Consulta>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    public async Task BuscarRetornosProximosAsync_JanelaInvalida_DeveLancarRegraDeNegocio(int dias)
    {
        // Act
        var acao = () => _sut.BuscarRetornosProximosAsync(dias);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
    }

    [Fact]
    public async Task BuscarRetornosProximosAsync_JanelaValida_DeveConsultarDeHojeAteNDias()
    {
        // Arrange
        _consultas.Setup(r => r.ListarRetornosEntreAsync(DateTime.Today, DateTime.Today.AddDays(7), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new[] { EntityBuilder.NovaConsulta(id: 1) });

        // Act
        var resultado = await _sut.BuscarRetornosProximosAsync(7);

        // Assert
        Assert.Single(resultado);
    }

    [Fact]
    public async Task ListarAsync_DataInicialPosteriorAFinal_DeveLancarRegraDeNegocio()
    {
        // Arrange
        var query = new ConsultaQueryDto { DataInicio = new DateTime(2026, 12, 1), DataFim = new DateTime(2026, 11, 1) };

        // Act
        var acao = () => _sut.ListarAsync(query);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(acao);
    }

    [Fact]
    public async Task BuscarPorPetAsync_PetInexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _pets.Setup(r => r.ExisteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var acao = () => _sut.BuscarPorPetAsync(9);

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }
}
