using Clyvo.PetCare.Api.ExceptionHandling;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Api;

/// <summary>Tradução global de exceções para ProblemDetails (RFC 7807). Padrão AAA.</summary>
public class GlobalExceptionHandlerTests
{
    public static TheoryData<Exception, int> Casos => new()
    {
        { new RecursoNaoEncontradoException("Tutor", 1), StatusCodes.Status404NotFound },
        { new ConflitoException("CPF duplicado"), StatusCodes.Status409Conflict },
        { new DomainException("Regra violada"), StatusCodes.Status422UnprocessableEntity },
        { new RegraDeNegocioException("Tutor com pets"), StatusCodes.Status422UnprocessableEntity },
        { new CredenciaisInvalidasException(), StatusCodes.Status401Unauthorized },
        { new OperationCanceledException(), GlobalExceptionHandler.StatusClienteDesconectado },
        { new InvalidOperationException("bug"), StatusCodes.Status500InternalServerError }
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void Classificar_DeveMapearExcecaoParaStatusHttp(Exception excecao, int statusEsperado)
    {
        // Act
        var (status, titulo) = GlobalExceptionHandler.Classificar(excecao);

        // Assert
        Assert.Equal(statusEsperado, status);
        Assert.False(string.IsNullOrWhiteSpace(titulo));
    }

    [Fact]
    public async Task TryHandleAsync_RecursoNaoEncontrado_DeveEscreverProblemDetails404ComMensagem()
    {
        // Arrange
        var (handler, escritor) = CriarHandler(Environments.Production);
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/tutores/9";

        // Act
        var tratado = await handler.TryHandleAsync(contexto, new RecursoNaoEncontradoException("Tutor", 9), CancellationToken.None);

        // Assert
        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status404NotFound, contexto.Response.StatusCode);
        Assert.Equal(404, escritor.Ultimo!.ProblemDetails.Status);
        Assert.Contains("9", escritor.Ultimo.ProblemDetails.Detail);
        Assert.Equal("/api/v1/tutores/9", escritor.Ultimo.ProblemDetails.Instance);
    }

    [Fact]
    public async Task TryHandleAsync_ErroInesperadoEmProducao_NaoDeveExporDetalhesInternos()
    {
        // Arrange
        var (handler, escritor) = CriarHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(new DefaultHttpContext(), new InvalidOperationException("string de conexão secreta"), CancellationToken.None);

        // Assert
        Assert.Equal(500, escritor.Ultimo!.ProblemDetails.Status);
        Assert.DoesNotContain("secreta", escritor.Ultimo.ProblemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ErroInesperadoEmDesenvolvimento_DeveExporMensagemParaDiagnostico()
    {
        // Arrange
        var (handler, escritor) = CriarHandler(Environments.Development);

        // Act
        await handler.TryHandleAsync(new DefaultHttpContext(), new InvalidOperationException("detalhe útil"), CancellationToken.None);

        // Assert
        Assert.Equal("detalhe útil", escritor.Ultimo!.ProblemDetails.Detail);
    }

    private static (GlobalExceptionHandler Handler, ProblemDetailsServiceFake Escritor) CriarHandler(string ambiente)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(ambiente);

        var escritor = new ProblemDetailsServiceFake();
        var handler = new GlobalExceptionHandler(escritor, environment.Object, NullLogger<GlobalExceptionHandler>.Instance);
        return (handler, escritor);
    }

    /// <summary>Captura o ProblemDetails que seria escrito na resposta.</summary>
    private sealed class ProblemDetailsServiceFake : IProblemDetailsService
    {
        public ProblemDetailsContext? Ultimo { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Ultimo = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Ultimo = context;
            return ValueTask.FromResult(true);
        }
    }
}
