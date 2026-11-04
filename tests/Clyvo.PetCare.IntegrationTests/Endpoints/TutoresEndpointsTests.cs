using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Endpoints;

/// <summary>
/// Endpoints de tutores percorrendo o pipeline completo — autenticação, validação,
/// controller, serviço, repositório, EF e resposta com HATEOAS. Padrão AAA.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class TutoresEndpointsTests : ApiTestBase
{
    public TutoresEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_TutorValido_DeveRetornar201ComLocationELinksHateoas()
    {
        // Arrange
        var cliente = await ClienteAsync(PerfilUsuario.Atendente);

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = "Mariana Silva",
            Cpf = "123.456.789-01",
            Email = "mariana@email.com",
            Telefone = "11987654321"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criado = await LerAsync<TutorResponseDto>(resposta);
        Assert.Equal("12345678901", criado.Cpf);
        Assert.EndsWith($"/api/v1/tutores/{criado.Id}", resposta.Headers.Location!.ToString());

        var self = Assert.Single(criado.Links, l => l.Rel == "self");
        Assert.EndsWith($"/api/v1/tutores/{criado.Id}", self.Href);
        Assert.Contains(criado.Links, l => l.Rel == "pets" && l.Href.EndsWith($"/api/v1/pets/tutor/{criado.Id}"));
        Assert.Contains(criado.Links, l => l.Rel == "remover" && l.Metodo == "DELETE");
    }

    [Fact]
    public async Task Post_CpfDuplicado_DeveRetornar409ProblemDetails()
    {
        // Arrange
        var cliente = await ClienteAsync();
        await CriarTutorAsync(cliente, cpf: "99988877766");

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = "Cópia", Cpf = "999.888.777-66", Email = "copia@email.com", Telefone = "11911111111"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        using var problema = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.Equal(409, problema.RootElement.GetProperty("status").GetInt32());
        Assert.True(problema.RootElement.TryGetProperty("correlationId", out _));
        Assert.True(problema.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Post_DadosInvalidos_DeveRetornar400ComErrosDeValidacao()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/tutores", new { Telefone = "1" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var problema = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.True(problema.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Post_UfComDigitos_DeveRetornar422RegraDeDominio()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = "Mariana", Cpf = "12345678901", Email = "m@email.com", Telefone = "11987654321", Uf = "S1"
        });

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task GetPorId_Inexistente_DeveRetornar404ProblemDetails()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPorCpf_ComPontuacao_DeveEncontrarTutor()
    {
        // Arrange
        var cliente = await ClienteAsync();
        await CriarTutorAsync(cliente, cpf: "55544433322");

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores/cpf/555.444.333-22");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_ListaComOrdenacaoEPaginacao_DeveRespeitarParametrosEGerarLinksDeNavegacao()
    {
        // Arrange
        var cliente = await ClienteAsync();
        await CriarTutorAsync(cliente, cpf: "11111111111", nome: "Carla Souza");
        await CriarTutorAsync(cliente, cpf: "22222222222", nome: "Ana Lima");
        await CriarTutorAsync(cliente, cpf: "33333333333", nome: "Bruno Reis");

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores?ordenarPor=Nome&direcao=Desc&pagina=1&tamanhoPagina=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var pagina = await LerAsync<PagedResultDto<TutorResponseDto>>(resposta);
        Assert.Equal(new[] { "Carla Souza", "Bruno Reis" }, pagina.Itens.Select(t => t.Nome));
        Assert.Equal(3, pagina.TotalRegistros);
        Assert.Equal(2, pagina.TotalPaginas);

        var proxima = Assert.Single(pagina.Links, l => l.Rel == "proxima-pagina");
        Assert.Contains("pagina=2", proxima.Href);
        Assert.Contains("ordenarPor=Nome", proxima.Href);
        Assert.Contains("direcao=Desc", proxima.Href);
        Assert.DoesNotContain(pagina.Links, l => l.Rel == "pagina-anterior");
        Assert.All(pagina.Itens, t => Assert.Contains(t.Links, l => l.Rel == "self"));
    }

    [Fact]
    public async Task Get_FiltroPorCidade_DeveRetornarSomenteTutoresDaCidade()
    {
        // Arrange
        var cliente = await ClienteAsync();
        await CriarTutorAsync(cliente, cpf: "11111111111", cidade: "Campinas");
        await CriarTutorAsync(cliente, cpf: "22222222222", cidade: "Santos");

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores?cidade=campinas");

        // Assert
        var pagina = await LerAsync<PagedResultDto<TutorResponseDto>>(resposta);
        var unico = Assert.Single(pagina.Itens);
        Assert.Equal("Campinas", unico.Cidade);
    }

    [Fact]
    public async Task Get_TamanhoPaginaAcimaDoLimite_DeveRetornar400()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores?tamanhoPagina=500");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Put_TutorExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var criado = await CriarTutorAsync(cliente);

        // Act
        var resposta = await cliente.PutAsJsonAsync($"/api/v1/tutores/{criado.Id}", new AtualizarTutorDto
        {
            Nome = "Mariana Atualizada", Email = "nova@email.com", Telefone = "11900001111", AceitaNotificacoes = false
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atualizado = await LerAsync<TutorResponseDto>(resposta);
        Assert.Equal("Mariana Atualizada", atualizado.Nome);
        Assert.False(atualizado.AceitaNotificacoes);
    }

    [Fact]
    public async Task Delete_ComoAtendente_DeveRetornar403()
    {
        // Arrange
        var admin = await ClienteAsync();
        var criado = await CriarTutorAsync(admin);
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);

        // Act
        var resposta = await atendente.DeleteAsync($"/api/v1/tutores/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_TutorSemPets_DeveRetornar204EDepois404()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var criado = await CriarTutorAsync(cliente);

        // Act
        var resposta = await cliente.DeleteAsync($"/api/v1/tutores/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var busca = await cliente.GetAsync($"/api/v1/tutores/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
    }

    [Fact]
    public async Task Delete_TutorComPets_DeveRetornar422()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);
        await CriarPetAsync(cliente, tutor.Id);

        // Act
        var resposta = await cliente.DeleteAsync($"/api/v1/tutores/{tutor.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }
}
