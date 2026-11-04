using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Queries;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Registro e consulta da trilha de auditoria (MongoDB simulado). Padrão AAA.</summary>
public class AuditoriaServicesTests
{
    private readonly Mock<IAuditoriaRepository> _repositorio = new();
    private readonly Mock<IUsuarioAtual> _usuarioAtual = new();

    public AuditoriaServicesTests()
    {
        _usuarioAtual.SetupGet(u => u.Identificador).Returns("atendente@clyvo.com.br");
        _usuarioAtual.SetupGet(u => u.CorrelationId).Returns("corr-123");
    }

    private AuditoriaRegistrador CriarRegistrador()
        => new(_repositorio.Object, _usuarioAtual.Object, NullLogger<AuditoriaRegistrador>.Instance);

    [Fact]
    public async Task RegistrarAsync_DeveSerializarDadosEUsarUsuarioDaRequisicao()
    {
        // Arrange
        RegistroAuditoria? gravado = null;
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<RegistroAuditoria>(), It.IsAny<CancellationToken>()))
                    .Callback<RegistroAuditoria, CancellationToken>((registro, _) => gravado = registro)
                    .Returns(Task.CompletedTask);

        // Act
        await CriarRegistrador().RegistrarAsync("Pet", 15, AcaoAuditoria.Criacao, new { Nome = "Luna", Porte = PorteCachorro.Medio });

        // Assert
        Assert.NotNull(gravado);
        Assert.Equal("Pet", gravado!.Entidade);
        Assert.Equal("15", gravado.EntidadeId);
        Assert.Equal("atendente@clyvo.com.br", gravado.Usuario);
        Assert.Equal("corr-123", gravado.CorrelationId);
        Assert.Contains("\"nome\":\"Luna\"", gravado.DadosJson);
        Assert.Contains("\"porte\":\"Medio\"", gravado.DadosJson);
    }

    [Fact]
    public async Task RegistrarAsync_ComExecutorExplicito_DevePrevalecerSobreUsuarioAtual()
    {
        // Arrange
        RegistroAuditoria? gravado = null;
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<RegistroAuditoria>(), It.IsAny<CancellationToken>()))
                    .Callback<RegistroAuditoria, CancellationToken>((registro, _) => gravado = registro)
                    .Returns(Task.CompletedTask);

        // Act
        await CriarRegistrador().RegistrarAsync("Usuario", 1, AcaoAuditoria.Login, null, executadoPor: "admin@clyvo.com.br");

        // Assert
        Assert.Equal("admin@clyvo.com.br", gravado!.Usuario);
        Assert.Null(gravado.DadosJson);
    }

    [Fact]
    public async Task RegistrarAsync_MongoIndisponivel_NaoDevePropagarErro()
    {
        // Arrange
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<RegistroAuditoria>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new TimeoutException("Mongo fora do ar"));

        // Act
        var erro = await Record.ExceptionAsync(() =>
            CriarRegistrador().RegistrarAsync("Tutor", 1, AcaoAuditoria.Remocao, null));

        // Assert
        Assert.Null(erro);
    }

    [Fact]
    public async Task ListarAsync_DeveConverterDadosJsonEmDocumento()
    {
        // Arrange
        var registro = RegistroAuditoria.Reconstituir("6720f0c2a1b2c3d4e5f60718", "Tutor", "3", AcaoAuditoria.Criacao,
            "admin@clyvo.com.br", DateTime.UtcNow, "corr", "{\"nome\":\"Mariana\"}");
        _repositorio.Setup(r => r.ListarAsync(It.IsAny<AuditoriaFiltro>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ResultadoPaginado<RegistroAuditoria>(new[] { registro }, 1));
        var sut = new AuditoriaConsultaService(_repositorio.Object);

        // Act
        var pagina = await sut.ListarAsync(new AuditoriaQueryDto { Entidade = "Tutor" });

        // Assert
        var item = Assert.Single(pagina.Itens);
        Assert.Equal("Mariana", item.Dados!.Value.GetProperty("nome").GetString());
        Assert.Equal(1, pagina.TotalPaginas);
    }

    [Fact]
    public async Task BuscarPorIdAsync_Inexistente_DeveLancarRecursoNaoEncontrado()
    {
        // Arrange
        _repositorio.Setup(r => r.ObterPorIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync((RegistroAuditoria?)null);
        var sut = new AuditoriaConsultaService(_repositorio.Object);

        // Act
        var acao = () => sut.BuscarPorIdAsync("abc");

        // Assert
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(acao);
    }

    [Fact]
    public async Task ResumirAsync_DeveMapearTotaisAgregados()
    {
        // Arrange
        _repositorio.Setup(r => r.ResumirPorEntidadeEAcaoAsync(null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new[] { new ResumoAuditoria("Pet", AcaoAuditoria.Criacao, 4) });
        var sut = new AuditoriaConsultaService(_repositorio.Object);

        // Act
        var resumo = await sut.ResumirAsync(null);

        // Assert
        var linha = Assert.Single(resumo);
        Assert.Equal("Pet", linha.Entidade);
        Assert.Equal(4, linha.Total);
    }
}
