using System.Net;
using System.Text.Json;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Observability;

/// <summary>
/// Endpoints de observabilidade: health checks (liveness/readiness com Oracle e
/// MongoDB), métricas Prometheus, correlation id e documentação OpenAPI. Padrão AAA.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class HealthCheckEndpointsTests : ApiTestBase
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ApiSaudavel_DeveRetornar200ComTodosOsChecks()
    {
        // Act
        var resposta = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", documento.RootElement.GetProperty("status").GetString());

        var checks = documento.RootElement.GetProperty("entries");
        Assert.True(checks.TryGetProperty("self", out _));
        Assert.True(checks.TryGetProperty("oracle-database", out _));
        Assert.True(checks.TryGetProperty("mongodb", out _));
    }

    [Fact]
    public async Task GetHealthLive_DeveVerificarSomenteOProcesso()
    {
        // Act
        var resposta = await _client.GetAsync("/health/live");
        var corpo = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("self", corpo);
        Assert.DoesNotContain("oracle-database", corpo);
    }

    [Fact]
    public async Task GetHealthReady_DeveVerificarOracleEMongo()
    {
        // Act
        var resposta = await _client.GetAsync("/health/ready");
        var corpo = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("oracle-database", corpo);
        Assert.Contains("mongodb", corpo);
    }

    [Fact]
    public async Task GetMetrics_DeveRetornar200NoFormatoPrometheus()
    {
        // Arrange — gera tráfego para haver medições antes do scrape.
        await _client.GetAsync("/health/live");
        await _client.GetAsync("/api/v1/tutores");

        // Act
        var resposta = await _client.GetAsync("/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("# TYPE", corpo);
    }

    [Fact]
    public async Task Requisicao_ComCorrelationId_DeveEcoarOHeaderNaResposta()
    {
        // Arrange
        const string correlationId = "teste-correlation-abc";
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        requisicao.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        var resposta = await _client.SendAsync(requisicao);

        // Assert
        Assert.True(resposta.Headers.TryGetValues("X-Correlation-Id", out var valores));
        Assert.Equal(correlationId, valores!.Single());
    }

    [Fact]
    public async Task Requisicao_SemCorrelationId_DeveGerarUmNovo()
    {
        // Act
        var resposta = await _client.GetAsync("/health/live");

        // Assert
        Assert.True(resposta.Headers.TryGetValues("X-Correlation-Id", out var valores));
        Assert.False(string.IsNullOrWhiteSpace(valores!.Single()));
    }

    [Fact]
    public async Task Swagger_DeveDocumentarEsquemaBearerEEndpointsDaSprint4()
    {
        // Act
        var resposta = await _client.GetAsync("/swagger/v1/swagger.json");
        var corpo = await resposta.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("\"Bearer\"", corpo);
        Assert.Contains("/api/v1/auth/login", corpo);
        Assert.Contains("/api/v1/auditoria", corpo);
        Assert.Contains("ordenarPor", corpo);
    }

    [Fact]
    public async Task RotaInexistente_DeveRetornar404ComoProblemDetails()
    {
        // Act
        var resposta = await _client.GetAsync("/api/v1/rota-que-nao-existe");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }
}
