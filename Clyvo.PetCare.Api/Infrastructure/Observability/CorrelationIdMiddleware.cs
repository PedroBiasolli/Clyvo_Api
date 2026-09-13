using Serilog.Context;

namespace Clyvo.PetCare.Api.Infrastructure.Observability;

/// <summary>
/// Middleware que garante um identificador único (CorrelationId) por requisição.
/// Lê do header <c>X-Correlation-Id</c> se presente ou gera um GUID novo.
/// O valor é propagado para os logs estruturados via <see cref="LogContext"/>
/// e devolvido ao cliente no header de resposta para rastreabilidade fim-a-fim.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var incoming)
                            && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Propaga o CorrelationId para todos os logs estruturados emitidos
        // durante o pipeline dessa requisição.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
