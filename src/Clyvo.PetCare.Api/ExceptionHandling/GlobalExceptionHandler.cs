using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Clyvo.PetCare.Api.ExceptionHandling;

/// <summary>
/// Tratamento global e consistente de exceções: toda falha vira um
/// <see cref="ProblemDetails"/> (RFC 7807) com status, título, detalhe,
/// traceId e correlationId — e os controllers ficam sem try/catch.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public const int StatusClienteDesconectado = 499;
    private const string MensagemErroInterno = "Ocorreu um erro inesperado. Informe o correlationId ao suporte.";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo) = Classificar(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro não tratado em {Metodo} {Caminho}", httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("Requisição {Metodo} {Caminho} rejeitada com {StatusCode}: {Mensagem}",
                httpContext.Request.Method, httpContext.Request.Path, status, exception.Message);

        httpContext.Response.StatusCode = status;

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = status == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
                ? MensagemErroInterno
                : exception.Message,
            Type = $"https://httpstatuses.io/{status}",
            Instance = httpContext.Request.Path
        };

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
            Exception = exception
        });
    }

    /// <summary>Tabela única de tradução exceção → (status HTTP, título).</summary>
    public static (int Status, string Titulo) Classificar(Exception exception) => exception switch
    {
        RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
        ConflitoException => (StatusCodes.Status409Conflict, "Conflito com o estado atual do recurso"),
        DomainException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
        RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
        CredenciaisInvalidasException => (StatusCodes.Status401Unauthorized, "Não autenticado"),
        OperationCanceledException => (StatusClienteDesconectado, "Requisição cancelada pelo cliente"),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
    };
}
