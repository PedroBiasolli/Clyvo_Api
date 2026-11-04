using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clyvo.PetCare.Api.ExceptionHandling;
using Clyvo.PetCare.Api.Hateoas;
using Clyvo.PetCare.Api.Middlewares;
using Serilog;

namespace Clyvo.PetCare.Api.Configuration;

public static class ApiConfiguration
{
    /// <summary>Controllers, JSON, ProblemDetails, tratamento global de exceções, HATEOAS e CORS.</summary>
    public static IServiceCollection AddApresentacao(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = contexto =>
            {
                var http = contexto.HttpContext;
                contexto.ProblemDetails.Instance ??= http.Request.Path;
                contexto.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;

                if (http.Items[CorrelationIdMiddleware.ItemKey] is string correlationId)
                    contexto.ProblemDetails.Extensions["correlationId"] = correlationId;
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ILinkService, LinkService>();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

        return services;
    }

    /// <summary>Ordem do pipeline HTTP — cada etapa depende das anteriores.</summary>
    public static WebApplication UsePipelineClyvo(this WebApplication app)
    {
        // 1. CorrelationId primeiro: tudo o que vem depois já loga com o ID da requisição.
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. Uma linha de log estruturado por requisição (método, rota, status, duração).
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnostico, http) =>
            {
                diagnostico.Set("RequestHost", http.Request.Host.Value);
                diagnostico.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
                diagnostico.Set("Usuario", http.User.Identity?.IsAuthenticated == true ? http.User.Identity.Name : "anonimo");
            };
        });

        // 3. Exceções → ProblemDetails; respostas vazias 4xx/5xx (401, 403, 404 de rota) → ProblemDetails.
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // 4. Documentação interativa.
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clyvo Pet Care API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Clyvo Pet Care — API Docs";
            options.EnablePersistAuthorization();
        });

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpointsDeObservabilidade();
        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }
}
