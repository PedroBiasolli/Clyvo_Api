using System.Net;
using System.Net.Http.Json;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Endpoints;

/// <summary>Endpoints de pets ponta a ponta. Padrão AAA.</summary>
[Collection(ApiTestCollection.Name)]
public class PetsEndpointsTests : ApiTestBase
{
    public PetsEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_PetValido_DeveRetornar201ComLinksParaTutorConsultasEVacinas()
    {
        // Arrange
        var cliente = await ClienteAsync(PerfilUsuario.Atendente);
        var tutor = await CriarTutorAsync(cliente);

        // Act
        var pet = await CriarPetAsync(cliente, tutor.Id, microchip: "982000123456789");

        // Assert
        Assert.Equal(tutor.Id, pet.TutorId);
        Assert.Equal(tutor.Nome, pet.TutorNome);
        Assert.Equal(new[] { "self", "atualizar", "remover", "tutor", "consultas", "vacinas" }, pet.Links.Select(l => l.Rel));
    }

    [Fact]
    public async Task Post_TutorInexistente_DeveRetornar422()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = "Órfão", Raca = "SRD", DataNascimento = new DateTime(2020, 1, 1),
            Sexo = Sexo.Macho, PesoKg = 10m, Porte = PorteCachorro.Medio, TutorId = 9999
        });

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Post_MicrochipDuplicado_DeveRetornar409()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);
        await CriarPetAsync(cliente, tutor.Id, nome: "Thor", microchip: "CHIP-001");

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = "Clone", Raca = "SRD", DataNascimento = new DateTime(2020, 1, 1),
            Sexo = Sexo.Macho, PesoKg = 10m, Porte = PorteCachorro.Medio, Microchip = "CHIP-001", TutorId = tutor.Id
        });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Post_NascimentoFuturo_DeveRetornar422()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = "Futuro", Raca = "SRD", DataNascimento = DateTime.Today.AddDays(10),
            Sexo = Sexo.Femea, PesoKg = 5m, Porte = PorteCachorro.Pequeno, TutorId = tutor.Id
        });

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task GetPorTutor_DeveRetornarColecaoComLinkDoTutor()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);
        await CriarPetAsync(cliente, tutor.Id, nome: "Bento");
        await CriarPetAsync(cliente, tutor.Id, nome: "Amora");

        // Act
        var resposta = await cliente.GetAsync($"/api/v1/pets/tutor/{tutor.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var colecao = await LerAsync<ColecaoDto<PetResponseDto>>(resposta);
        Assert.Equal(new[] { "Amora", "Bento" }, colecao.Itens.Select(p => p.Nome));
        Assert.Contains(colecao.Links, l => l.Rel == "tutor");
    }

    [Fact]
    public async Task GetPorMicrochip_Existente_DeveRetornar200()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);
        await CriarPetAsync(cliente, tutor.Id, microchip: "985112233445566");

        // Act
        var resposta = await cliente.GetAsync("/api/v1/pets/microchip/985112233445566");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_FiltroPorPorteOrdenadoPorPesoDecrescente_DeveRespeitarFiltroEOrdem()
    {
        // Arrange
        var cliente = await ClienteAsync();
        var tutor = await CriarTutorAsync(cliente);
        await CriarPetAsync(cliente, tutor.Id, nome: "Leve", pesoKg: 26m, porte: PorteCachorro.Grande);
        await CriarPetAsync(cliente, tutor.Id, nome: "Pesado", pesoKg: 40m, porte: PorteCachorro.Grande);
        await CriarPetAsync(cliente, tutor.Id, nome: "Mini", pesoKg: 3m, porte: PorteCachorro.Mini);

        // Act
        var resposta = await cliente.GetAsync("/api/v1/pets?porte=Grande&ordenarPor=PesoKg&direcao=Desc");

        // Assert
        var pagina = await LerAsync<PagedResultDto<PetResponseDto>>(resposta);
        Assert.Equal(new[] { "Pesado", "Leve" }, pagina.Itens.Select(p => p.Nome));
    }

    [Fact]
    public async Task Get_OrdenacaoInexistente_DeveRetornar400()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.GetAsync("/api/v1/pets?ordenarPor=CampoQueNaoExiste");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
