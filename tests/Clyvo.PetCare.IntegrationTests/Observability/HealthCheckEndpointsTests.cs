using System.Net;
using System.Text.Json;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Observability;

/// <summary>
/// Testes de integração dos endpoints de Health Check e Metrics.
/// Validam que a API expõe corretamente os endpoints de observabilidade
/// esperados por sondas Kubernetes e coletores Prometheus.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class HealthCheckEndpointsTests
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(ClyvoWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ApiSaudavel_DeveRetornar200EStatusHealthy()
    {
        // Act
        var resposta = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var body = await resposta.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.Equal("Healthy", status);
    }

    [Fact]
    public async Task GetHealthLive_ApiSaudavel_DeveRetornar200()
    {
        // Act
        var resposta = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_ApiSaudavel_DeveRetornar200()
    {
        // Act
        var resposta = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task GetHealth_DeveIncluirNomesDosChecksNoBody()
    {
        // Act
        var resposta = await _client.GetAsync("/health");
        var body = await resposta.Content.ReadAsStringAsync();

        // Assert — o UIResponseWriter retorna JSON com os checks nomeados.
        Assert.Contains("self", body);
        Assert.Contains("oracle-database", body);
    }

    [Fact]
    public async Task GetMetrics_DeveRetornar200ComFormatoPrometheus()
    {
        // Arrange — gera tráfego para que haja métricas coletadas antes do scrape.
        // O exportador Prometheus pode devolver corpo vazio na primeira leitura
        // se nenhuma medição tiver sido registrada ainda.
        await _client.GetAsync("/api/v1/tutores");
        await _client.GetAsync("/api/v1/pets");

        // Act
        var resposta = await _client.GetAsync("/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var body = await resposta.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body), "O endpoint /metrics devolveu corpo vazio.");
        Assert.Contains("# TYPE", body);
    }

    [Fact]
    public async Task GetTutores_DeveEcoarHeaderCorrelationIdNaResposta()
    {
        // Arrange
        var correlationId = "test-correlation-id-abc";
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        // Act
        var resposta = await _client.GetAsync("/api/v1/tutores");

        // Assert
        Assert.True(resposta.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.Equal(correlationId, values!.Single());
    }
}
