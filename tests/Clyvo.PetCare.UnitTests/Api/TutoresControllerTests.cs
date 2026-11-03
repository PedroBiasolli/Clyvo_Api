using Clyvo.PetCare.Api.Controllers;
using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Clyvo.PetCare.UnitTests.Api;

/// <summary>
/// Controller isolado: delega ao serviço, devolve o status HTTP correto e
/// enriquece as respostas com links HATEOAS. Padrão AAA.
/// </summary>
public class TutoresControllerTests
{
    private readonly Mock<ITutorService> _service = new(MockBehavior.Strict);
    private readonly Mock<ILinkService> _links = new();
    private readonly TutoresController _sut;

    public TutoresControllerTests()
    {
        _links.Setup(l => l.Gerar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
              .Returns<string, string, object?>((acao, controlador, _) => $"http://localhost/{controlador}/{acao}");

        _sut = new TutoresController(_service.Object, _links.Object);
    }

    [Fact]
    public async Task BuscarPorId_TutorExistente_DeveRetornarOkComLinksHateoas()
    {
        // Arrange
        _service.Setup(s => s.BuscarPorIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TutorResponseDto { Id = 1, Nome = "Mariana" });

        // Act
        var resposta = await _sut.BuscarPorId(1, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var tutor = Assert.IsType<TutorResponseDto>(ok.Value);
        Assert.Equal(new[] { "self", "atualizar", "remover", "pets" }, tutor.Links.Select(l => l.Rel));
        Assert.Contains(tutor.Links, l => l.Rel == "remover" && l.Metodo == "DELETE");
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornarCreatedAtActionDoBuscarPorId()
    {
        // Arrange
        var dto = new CriarTutorDto { Nome = "Ana", Cpf = "12345678901", Email = "a@x.com", Telefone = "11999990000" };
        _service.Setup(s => s.CriarAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TutorResponseDto { Id = 42, Nome = "Ana" });

        // Act
        var resposta = await _sut.Criar(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resposta.Result);
        Assert.Equal(nameof(TutoresController.BuscarPorId), created.ActionName);
        Assert.Equal(42, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Listar_DeveIncluirLinksDeNavegacaoDaPagina()
    {
        // Arrange
        var query = new TutorQueryDto { Pagina = 2, TamanhoPagina = 10 };
        _service.Setup(s => s.ListarAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResultDto<TutorResponseDto>
                {
                    Itens = new[] { new TutorResponseDto { Id = 1 } },
                    Pagina = 2,
                    TamanhoPagina = 10,
                    TotalRegistros = 35
                });

        // Act
        var resposta = await _sut.Listar(query, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var pagina = Assert.IsType<PagedResultDto<TutorResponseDto>>(ok.Value);
        Assert.Equal(
            new[] { "self", "primeira-pagina", "pagina-anterior", "proxima-pagina", "ultima-pagina" },
            pagina.Links.Select(l => l.Rel));
        Assert.NotEmpty(pagina.Itens[0].Links);
    }

    [Fact]
    public async Task Remover_DeveDelegarAoServicoERetornarNoContent()
    {
        // Arrange
        _service.Setup(s => s.RemoverAsync(10, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var resposta = await _sut.Remover(10, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resposta);
        _service.Verify(s => s.RemoverAsync(10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
