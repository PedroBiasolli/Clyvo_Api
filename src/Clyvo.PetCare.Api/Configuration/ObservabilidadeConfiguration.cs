using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Clyvo.PetCare.Infrastructure;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Clyvo.PetCare.Api.Configuration;

public static class ObservabilidadeConfiguration
{
    public const string NomeServico = "Clyvo.PetCare.Api";
    public const string VersaoServico = "4.0.0";

    /// <summary>
    /// Logging estruturado com Serilog: console + arquivo diário rotativo,
    /// enriquecido com CorrelationId, máquina, thread e aplicação.
    /// Níveis e sinks configuráveis em appsettings (seção "Serilog").
    /// </summary>
    public static WebApplicationBuilder AddLoggingEstruturado(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", NomeServico));

        return builder;
    }

    /// <summary>Health checks (self, Oracle, MongoDB) + OpenTelemetry (tracing e métricas Prometheus).</summary>
    public static IServiceCollection AddObservabilidade(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API respondendo"), tags: new[] { "live" })
            .AddInfrastructureHealthChecks();

        var recurso = ResourceBuilder.CreateDefault().AddService(NomeServico, serviceVersion: VersaoServico);

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(recurso)
                .AddSource(NomeServico)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = httpContext =>
                        !httpContext.Request.Path.StartsWithSegments("/health") &&
                        !httpContext.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                    options.SetDbStatementForText = environment.IsDevelopment())
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(recurso)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter());

        return services;
    }

    /// <summary>
    /// /health (completo), /health/live (liveness) e /health/ready (readiness: Oracle + MongoDB),
    /// além do scrape Prometheus em /metrics.
    /// </summary>
    public static WebApplication MapEndpointsDeObservabilidade(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();

        app.MapHealthChecks("/health", CriarOpcoes(_ => true)).AllowAnonymous();
        app.MapHealthChecks("/health/live", CriarOpcoes(check => check.Tags.Contains("live"))).AllowAnonymous();
        app.MapHealthChecks("/health/ready", CriarOpcoes(check => check.Tags.Contains("ready"))).AllowAnonymous();

        return app;
    }

    private static HealthCheckOptions CriarOpcoes(Func<HealthCheckRegistration, bool> filtro) => new()
    {
        Predicate = filtro,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    };
}
