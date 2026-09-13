using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Clyvo.PetCare.UnitTests.Controllers;

/// <summary>
/// Testes unitários do <see cref="TutoresController"/> — camada web isolada
/// via Moq da interface do serviço. Verificam o contrato HTTP: status codes,
/// tipos retornados e mapeamento de exceções para BadRequest.
/// </summary>
public class TutoresControllerTests
{
    private readonly Mock<ITutorService> _serviceMock = new(MockBehavior.Strict);

    // ────────────────────────────── BuscarPorId ──────────────────────────────

    [Fact]
    public async Task BuscarPorId_TutorExistente_DeveRetornarOkComDto()
    {
        // Arrange
        var dto = new TutorResponseDto { Id = 1, Nome = "Mariana", Cpf = "111", Email = "m@x.com" };
        _serviceMock.Setup(s => s.BuscarPorIdAsync(1)).ReturnsAsync(dto);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.BuscarPorId(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        Assert.Equal(dto, ok.Value);
        _serviceMock.Verify(s => s.BuscarPorIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task BuscarPorId_TutorInexistente_DeveRetornarNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.BuscarPorIdAsync(999)).ReturnsAsync((TutorResponseDto?)null);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.BuscarPorId(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resposta.Result);
    }

    // ────────────────────────────── Criar ──────────────────────────────

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornarCreatedAtAction()
    {
        // Arrange
        var input = new CriarTutorDto { Nome = "Ana", Cpf = "222", Email = "a@x.com", Telefone = "11" };
        var output = new TutorResponseDto { Id = 42, Nome = "Ana", Cpf = "222" };
        _serviceMock.Setup(s => s.CriarAsync(input)).ReturnsAsync(output);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Criar(input);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resposta.Result);
        Assert.Equal(nameof(TutoresController.BuscarPorId), created.ActionName);
        Assert.Equal(42, ((TutorResponseDto)created.Value!).Id);
    }

    [Fact]
    public async Task Criar_ServiceLancaInvalidOperation_DeveRetornarBadRequest()
    {
        // Arrange
        var input = new CriarTutorDto { Nome = "Dup", Cpf = "333", Email = "d@x.com", Telefone = "11" };
        _serviceMock.Setup(s => s.CriarAsync(input))
                    .ThrowsAsync(new InvalidOperationException("CPF duplicado"));
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Criar(input);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resposta.Result);
    }

    // ────────────────────────────── Listar ──────────────────────────────

    [Fact]
    public async Task Listar_ComResultado_DeveDelegarParaServiceERetornarOk()
    {
        // Arrange
        var pagina = new PagedResultDto<TutorResponseDto>
        {
            Itens = new[] { new TutorResponseDto { Id = 1, Nome = "X" } },
            Pagina = 1, TamanhoPagina = 20, TotalRegistros = 1
        };
        _serviceMock.Setup(s => s.ListarAsync(null, null, 1, 20)).ReturnsAsync(pagina);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Listar(null, null, 1, 20);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        Assert.Equal(pagina, ok.Value);
    }

    // ────────────────────────────── Atualizar / Remover ──────────────────────────────

    [Fact]
    public async Task Atualizar_TutorInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var dto = new AtualizarTutorDto { Nome = "X", Email = "x@x.com", Telefone = "1" };
        _serviceMock.Setup(s => s.AtualizarAsync(50, dto)).ReturnsAsync((TutorResponseDto?)null);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Atualizar(50, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resposta.Result);
    }

    [Fact]
    public async Task Remover_TutorSemPets_DeveRetornarNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.RemoverAsync(10)).ReturnsAsync(true);
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Remover(10);

        // Assert
        Assert.IsType<NoContentResult>(resposta);
    }

    [Fact]
    public async Task Remover_ServiceLancaInvalidOperation_DeveRetornarBadRequest()
    {
        // Arrange
        _serviceMock.Setup(s => s.RemoverAsync(11))
                    .ThrowsAsync(new InvalidOperationException("tutor com pets"));
        var controller = new TutoresController(_serviceMock.Object);

        // Act
        var resposta = await controller.Remover(11);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resposta);
    }
}
