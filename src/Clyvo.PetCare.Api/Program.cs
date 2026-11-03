using Clyvo.PetCare.Api.Configuration;
using Clyvo.PetCare.Application;
using Clyvo.PetCare.Infrastructure;
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

    builder.AddLoggingEstruturado();

    // Composition root — cada camada registra os próprios serviços.
    builder.Services
        .AddApplication()                                                   // casos de uso
        .AddInfrastructure(builder.Configuration, builder.Environment)      // Oracle, MongoDB, JWT, hash
        .AddApresentacao()                                                  // controllers, ProblemDetails, HATEOAS
        .AddSegurancaJwt()                                                  // autenticação e autorização
        .AddDocumentacaoSwagger()                                           // OpenAPI
        .AddObservabilidade(builder.Environment);                           // health checks + OpenTelemetry

    var app = builder.Build();

    app.UsePipelineClyvo();

    Log.Information("Clyvo Pet Care API pronta em {Environment}", app.Environment.EnvironmentName);
    app.Run();

    Log.CloseAndFlush();
}
// Exceções de controle do host NÃO são falhas e não podem ser capturadas aqui:
//  • HostAbortedException  → shutdown solicitado (ex.: dotnet ef durante design-time)
//  • StopTheHostException  → sentinela interna que o HostFactoryResolver lança para
//    interromper este entry point e capturar o IHost nos testes de integração.
catch (Exception ex) when (ex is not HostAbortedException
                           && ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, "A API terminou inesperadamente na inicialização");
    Log.CloseAndFlush();
}

// Torna a classe Program pública para o WebApplicationFactory dos testes de integração.
public partial class Program { }
