using System.Net;
using System.Net.Http.Json;
using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Controllers;

/// <summary>
/// Testes de integração dos endpoints de tutores. Percorrem o pipeline HTTP
/// completo — model binding, validação, controller, serviço, EF, resposta —
/// usando <see cref="ClyvoWebApplicationFactory"/>.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class TutoresEndpointsTests
{
    private readonly ClyvoWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TutoresEndpointsTests(ClyvoWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_TutorValido_DeveRetornar201ComLocation()
    {
        // Arrange
        var payload = new CriarTutorDto
        {
            Nome = "Mariana Silva",
            Cpf = "12345678901",
            Email = "mariana@email.com",
            Telefone = "11987654321",
            AceitaNotificacoes = true
        };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/v1/tutores", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var criado = await resposta.Content.ReadFromJsonAsync<TutorResponseDto>(JsonTestOptions.Default);
        Assert.NotNull(criado);
        Assert.True(criado!.Id > 0);
        Assert.Equal("Mariana Silva", criado.Nome);
    }

    [Fact]
    public async Task Post_CpfDuplicado_DeveRetornar400()
    {
        // Arrange — primeiro tutor
        var primeiro = new CriarTutorDto
        {
            Nome = "Original",
            Cpf = "99988877766",
            Email = "orig@email.com",
            Telefone = "11900000000"
        };
        await _client.PostAsJsonAsync("/api/v1/tutores", primeiro);

        var duplicado = new CriarTutorDto
        {
            Nome = "Cópia",
            Cpf = "99988877766",
            Email = "copia@email.com",
            Telefone = "11911111111"
        };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/v1/tutores", duplicado);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Post_DadosInvalidos_DeveRetornar400DoModelBinding()
    {
        // Arrange — sem nome nem CPF nem email
        var invalido = new { Telefone = "1" };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/v1/tutores", invalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task GetPorId_TutorInexistente_DeveRetornar404()
    {
        // Act
        var resposta = await _client.GetAsync("/api/v1/tutores/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task GetPorId_TutorExistente_DeveRetornar200ComDto()
    {
        // Arrange
        var payload = new CriarTutorDto
        {
            Nome = "Consulta",
            Cpf = "11122233344",
            Email = "c@x.com",
            Telefone = "11999990000"
        };
        var criacao = await _client.PostAsJsonAsync("/api/v1/tutores", payload);
        var criado = await criacao.Content.ReadFromJsonAsync<TutorResponseDto>(JsonTestOptions.Default);

        // Act
        var resposta = await _client.GetAsync($"/api/v1/tutores/{criado!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var lido = await resposta.Content.ReadFromJsonAsync<TutorResponseDto>(JsonTestOptions.Default);
        Assert.Equal(criado.Id, lido!.Id);
        Assert.Equal("Consulta", lido.Nome);
    }

    [Fact]
    public async Task GetPorCpf_CpfExistente_DeveRetornar200()
    {
        // Arrange
        var payload = new CriarTutorDto
        {
            Nome = "Busca CPF",
            Cpf = "55544433322",
            Email = "cpf@x.com",
            Telefone = "11999990000"
        };
        await _client.PostAsJsonAsync("/api/v1/tutores", payload);

        // Act
        var resposta = await _client.GetAsync("/api/v1/tutores/cpf/55544433322");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_ListaTutores_DeveRetornar200ComPaginacao()
    {
        // Arrange
        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
            {
                Nome = $"Tutor {i}",
                Cpf = $"7770000000{i}",
                Email = $"t{i}@x.com",
                Telefone = "11999990000"
            });
        }

        // Act
        var resposta = await _client.GetAsync("/api/v1/tutores?pagina=1&tamanhoPagina=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var lista = await resposta.Content.ReadFromJsonAsync<PagedResultDto<TutorResponseDto>>(JsonTestOptions.Default);
        Assert.NotNull(lista);
        Assert.Equal(3, lista!.TotalRegistros);
    }

    [Fact]
    public async Task Put_TutorExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var criado = await CriarTutorAsync(cpf: "88877766655", nome: "Antes");

        var update = new AtualizarTutorDto
        {
            Nome = "Depois",
            Email = criado.Email,
            Telefone = criado.Telefone,
            AceitaNotificacoes = false
        };

        // Act
        var resposta = await _client.PutAsJsonAsync($"/api/v1/tutores/{criado.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atualizado = await resposta.Content.ReadFromJsonAsync<TutorResponseDto>(JsonTestOptions.Default);
        Assert.Equal("Depois", atualizado!.Nome);
        Assert.False(atualizado.AceitaNotificacoes);
    }

    [Fact]
    public async Task Delete_TutorSemPets_DeveRetornar204()
    {
        // Arrange
        var criado = await CriarTutorAsync(cpf: "66655544433", nome: "Vou sumir");

        // Act
        var resposta = await _client.DeleteAsync($"/api/v1/tutores/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        // Confirma que a busca subsequente devolve 404.
        var busca = await _client.GetAsync($"/api/v1/tutores/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
    }

    private async Task<TutorResponseDto> CriarTutorAsync(string cpf, string nome)
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = nome,
            Cpf = cpf,
            Email = $"{cpf}@x.com",
            Telefone = "11999990000"
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<TutorResponseDto>(JsonTestOptions.Default))!;
    }
}
