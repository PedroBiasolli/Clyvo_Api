using Serilog.Context;

namespace Clyvo.PetCare.Api.Middlewares;

/// <summary>
/// Garante um identificador único (CorrelationId) por requisição.
/// Lê do header <c>X-Correlation-Id</c> se presente ou gera um novo GUID.
/// O valor é propagado para os logs estruturados (<see cref="LogContext"/>),
/// para a trilha de auditoria (via <c>HttpContext.Items</c>), para os
/// ProblemDetails de erro e devolvido no header de resposta.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string Header = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(Header, out var recebido)
                            && !string.IsNullOrWhiteSpace(recebido)
            ? recebido.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[ItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[Header] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(ItemKey, correlationId))
        {
            await _next(context);
        }
    }
}
