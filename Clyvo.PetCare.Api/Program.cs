using System.Reflection;
using System.Text.Json.Serialization;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Infrastructure.Observability;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

// ───────────────────────────────────────────────────────────────────────────────
// Bootstrap logger — captura falhas de startup antes do host subir.
// Usa CreateLogger (não CreateBootstrapLogger): o WebApplicationFactory dos
// testes de integração reexecuta este entry point a cada host criado, e um
// ReloadableLogger só pode ser congelado uma vez por processo.
// ───────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando a Clyvo Pet Care API");

    var builder = WebApplication.CreateBuilder(args);

    // ───────────────────────────────────────────────────────────────────────────────
    // Logging estruturado — Serilog
    // Console (dev) + arquivo rotativo diário em ./logs/clyvo-api-.log
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "Clyvo.PetCare.Api"));

    // ───────────────────────────────────────────────────────────────────────────────
    // Controllers + serialização JSON
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Services
        .AddControllers()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    builder.Services.AddEndpointsApiExplorer();

    // ───────────────────────────────────────────────────────────────────────────────
    // Banco de dados — Oracle FIAP
    // ───────────────────────────────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("OracleFiap")
        ?? throw new InvalidOperationException("Connection string 'OracleFiap' não configurada em appsettings.json.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(connectionString, oracle =>
                oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19))
               .EnableDetailedErrors(builder.Environment.IsDevelopment()));

    // ───────────────────────────────────────────────────────────────────────────────
    // Services (camada de aplicação)
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Services.AddScoped<ITutorService, TutorService>();
    builder.Services.AddScoped<IPetService, PetService>();
    builder.Services.AddScoped<IVeterinarioService, VeterinarioService>();
    builder.Services.AddScoped<IConsultaService, ConsultaService>();
    builder.Services.AddScoped<IVacinaService, VacinaService>();

    // ───────────────────────────────────────────────────────────────────────────────
    // Health Checks
    // Endpoints:
    //   /health/live  → liveness (a aplicação está de pé?)
    //   /health/ready → readiness (dependências prontas: DB conectado?)
    //   /health      → checagem completa (todos os checks)
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API respondendo"),
                  tags: new[] { "live" })
        .AddDbContextCheck<AppDbContext>(
            name: "oracle-database",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "ready", "db", "oracle" });

    // ───────────────────────────────────────────────────────────────────────────────
    // OpenTelemetry — Tracing + Métricas
    // Tracing: instrumenta ASP.NET Core, HttpClient e EF Core, exporta para Console.
    // Métricas: coletas de runtime, processo, ASP.NET, HttpClient — expostas
    //           em /metrics no formato Prometheus.
    // ───────────────────────────────────────────────────────────────────────────────
    const string ServiceName = "Clyvo.PetCare.Api";
    const string ServiceVersion = "3.0.0";

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion);

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .SetResourceBuilder(resourceBuilder)
            .AddSource(ServiceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                // Ignora endpoints de infra do próprio observability
                options.Filter = httpContext =>
                    !httpContext.Request.Path.StartsWithSegments("/health") &&
                    !httpContext.Request.Path.StartsWithSegments("/metrics");
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = builder.Environment.IsDevelopment();
            })
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter());

    // ───────────────────────────────────────────────────────────────────────────────
    // Swagger / OpenAPI
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Clyvo Pet Care API",
            Version = "v1",
            Description = "API REST para continuidade do cuidado e engajamento na jornada de saúde de pets (cachorros). " +
                          "Sprint 3 — com observabilidade (Serilog + OpenTelemetry) e Health Checks. " +
                          "Desenvolvido para o Challenge FIAP 2026 — Clyvo VET.",
            Contact = new OpenApiContact
            {
                Name = "Equipe Clyvo Pet Care",
                Email = "contato@clyvo.com.br"
            },
            License = new OpenApiLicense { Name = "Uso acadêmico — FIAP" }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

        c.EnableAnnotations();
    });

    // ───────────────────────────────────────────────────────────────────────────────
    // CORS
    // ───────────────────────────────────────────────────────────────────────────────
    builder.Services.AddCors(opt =>
    {
        opt.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    });

    var app = builder.Build();

    // ───────────────────────────────────────────────────────────────────────────────
    // Pipeline HTTP
    // ───────────────────────────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clyvo Pet Care API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Clyvo Pet Care — API Docs";
        });
    }

    // CorrelationId antes de tudo — todos os middlewares seguintes já entram
    // no escopo do LogContext com o ID da requisição.
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Serilog request logging — uma linha por request com método, path, status, duração.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();

    // Prometheus scraping endpoint — /metrics
    app.MapPrometheusScrapingEndpoint();

    // Health Checks — três endpoints com filtros por tag
    var healthOptions = new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    };

    app.MapHealthChecks("/health", healthOptions).AllowAnonymous();
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    app.MapControllers();

    // Endpoint raiz amigável — redireciona para o Swagger
    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();

    Log.Information("Clyvo Pet Care API pronta em {Environment}", app.Environment.EnvironmentName);
    app.Run();

    // Encerramento normal — libera os sinks antes do processo sair.
    Log.CloseAndFlush();
}
// Exceções de controle do host NÃO são falhas e não podem ser capturadas aqui:
//  • HostAbortedException  → shutdown solicitado (ex.: dotnet ef durante design-time)
//  • StopTheHostException  → sentinela interna que o HostFactoryResolver lança para
//    interromper este entry point e capturar o IHost nos testes de integração.
//    Engolir essa exceção quebra o WebApplicationFactory com a mensagem
//    "The entry point exited without ever building an IHost".
catch (Exception ex) when (ex is not HostAbortedException
                           && ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, "A API terminou inesperadamente na inicialização");
    Log.CloseAndFlush();
}

// Torna a classe Program pública para o WebApplicationFactory dos testes de integração.
public partial class Program { }
