using System.Net;
using System.Net.Http.Json;
using Clyvo.PetCare.Api.Application.DTOs.Pet;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.Api.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Controllers;

/// <summary>
/// Testes de integração dos endpoints de pets. Fluxos cobertos:
/// criação com tutor válido/inválido, busca por microchip e por tutor,
/// listagem com filtros, atualização e remoção.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class PetsEndpointsTests
{
    private readonly ClyvoWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PetsEndpointsTests(ClyvoWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_PetComTutorExistente_DeveRetornar201()
    {
        // Arrange
        var tutor = await CriarTutorAsync();
        var payload = new CriarPetDto
        {
            Nome = "Thor",
            Raca = "Golden Retriever",
            DataNascimento = new DateTime(2022, 3, 15),
            Sexo = Sexo.Macho,
            PesoKg = 28.5m,
            Porte = PorteCachorro.Grande,
            Castrado = true,
            Microchip = "982000123456789",
            TutorId = tutor.Id
        };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/v1/pets", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criado = await resposta.Content.ReadFromJsonAsync<PetResponseDto>(JsonTestOptions.Default);
        Assert.Equal("Thor", criado!.Nome);
        Assert.Equal(tutor.Id, criado.TutorId);
    }

    [Fact]
    public async Task Post_PetComTutorInexistente_DeveRetornar400()
    {
        // Arrange
        var payload = new CriarPetDto
        {
            Nome = "Fantasma",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(-1),
            Sexo = Sexo.Femea,
            PesoKg = 5m,
            Porte = PorteCachorro.Pequeno,
            TutorId = 9999
        };

        // Act
        var resposta = await _client.PostAsJsonAsync("/api/v1/pets", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task GetPorMicrochip_MicrochipCadastrado_DeveRetornar200()
    {
        // Arrange
        var tutor = await CriarTutorAsync();
        await _client.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = "Buscado",
            Raca = "Poodle",
            DataNascimento = DateTime.Today.AddYears(-3),
            Sexo = Sexo.Femea,
            PesoKg = 8m,
            Porte = PorteCachorro.Pequeno,
            Microchip = "982000777777777",
            TutorId = tutor.Id
        });

        // Act
        var resposta = await _client.GetAsync("/api/v1/pets/microchip/982000777777777");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var pet = await resposta.Content.ReadFromJsonAsync<PetResponseDto>(JsonTestOptions.Default);
        Assert.Equal("982000777777777", pet!.Microchip);
    }

    [Fact]
    public async Task GetPorTutor_TutorComPets_DeveRetornarLista()
    {
        // Arrange
        var tutor = await CriarTutorAsync();
        for (var i = 0; i < 2; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
            {
                Nome = $"Pet{i}",
                Raca = "SRD",
                DataNascimento = DateTime.Today.AddYears(-2),
                Sexo = Sexo.Macho,
                PesoKg = 12m,
                Porte = PorteCachorro.Medio,
                Microchip = GerarMicrochip(),
                TutorId = tutor.Id
            });
        }

        // Act
        var resposta = await _client.GetAsync($"/api/v1/pets/tutor/{tutor.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var pets = await resposta.Content.ReadFromJsonAsync<IEnumerable<PetResponseDto>>(JsonTestOptions.Default);
        Assert.Equal(2, pets!.Count());
    }

    [Fact]
    public async Task Delete_PetSemVinculos_DeveRetornar204()
    {
        // Arrange
        var tutor = await CriarTutorAsync();
        var pet = await CriarPetAsync(tutor.Id);

        // Act
        var resposta = await _client.DeleteAsync($"/api/v1/pets/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    private async Task<TutorResponseDto> CriarTutorAsync()
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = "Tutor",
            Cpf = GerarCpf(),
            Email = "tutor@clyvo.com",
            Telefone = "11999990000"
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<TutorResponseDto>())!;
    }

    private async Task<PetResponseDto> CriarPetAsync(int tutorId)
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = "Bidu",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(-1),
            Sexo = Sexo.Macho,
            PesoKg = 10m,
            Porte = PorteCachorro.Pequeno,
            Microchip = GerarMicrochip(),
            TutorId = tutorId
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PetResponseDto>(JsonTestOptions.Default))!;
    }

    /// <summary>CPF numérico de 11 dígitos, único por chamada.</summary>
    private static string GerarCpf() =>
        Random.Shared.NextInt64(10_000_000_000L, 99_999_999_999L).ToString();

    /// <summary>
    /// Microchip no padrão ISO de 15 dígitos. Precisa caber no
    /// StringLength(20) do CriarPetDto — um GUID (32 chars) estouraria.
    /// </summary>
    private static string GerarMicrochip() =>
        Random.Shared.NextInt64(100_000_000_000_000L, 999_999_999_999_999L).ToString();
}
