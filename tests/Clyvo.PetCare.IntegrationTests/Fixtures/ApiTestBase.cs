using System.Net.Http.Json;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.IntegrationTests.Fixtures;

/// <summary>
/// Base dos testes de endpoint: reseta o banco a cada teste (xUnit cria uma
/// instância por teste) e oferece atalhos para montar o cenário via API.
/// </summary>
public abstract class ApiTestBase
{
    protected ApiTestBase(ClyvoWebApplicationFactory factory)
    {
        Factory = factory;
        Factory.ResetDatabase();
    }

    protected ClyvoWebApplicationFactory Factory { get; }

    protected Task<HttpClient> ClienteAsync(PerfilUsuario perfil = PerfilUsuario.Admin)
        => Factory.CriarClienteAutenticadoAsync(perfil);

    protected static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
        => (await resposta.Content.ReadFromJsonAsync<T>(JsonTestOptions.Default))!;

    protected static async Task<TutorResponseDto> CriarTutorAsync(HttpClient cliente, string cpf = "12345678901", string nome = "Mariana Silva", string? cidade = "São Paulo")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/tutores", new CriarTutorDto
        {
            Nome = nome,
            Cpf = cpf,
            Email = $"{cpf}@email.com",
            Telefone = "11987654321",
            Cidade = cidade,
            Uf = "SP"
        });
        resposta.EnsureSuccessStatusCode();
        return await LerAsync<TutorResponseDto>(resposta);
    }

    protected static async Task<PetResponseDto> CriarPetAsync(
        HttpClient cliente, int tutorId, string nome = "Thor", string? microchip = null,
        decimal pesoKg = 28.5m, PorteCachorro porte = PorteCachorro.Grande)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/pets", new CriarPetDto
        {
            Nome = nome,
            Raca = "Golden Retriever",
            DataNascimento = new DateTime(2022, 3, 15),
            Sexo = Sexo.Macho,
            PesoKg = pesoKg,
            Porte = porte,
            Microchip = microchip,
            Castrado = true,
            TutorId = tutorId
        }, JsonTestOptions.Default);
        resposta.EnsureSuccessStatusCode();
        return await LerAsync<PetResponseDto>(resposta);
    }

    protected static async Task<VeterinarioResponseDto> CriarVeterinarioAsync(HttpClient cliente, string crmv = "CRMV-SP 12345")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/veterinarios", new CriarVeterinarioDto
        {
            Nome = "Dra. Ana Lima",
            Crmv = crmv,
            Especialidade = "Clínica Geral",
            Email = "ana.lima@clyvo.com.br"
        });
        resposta.EnsureSuccessStatusCode();
        return await LerAsync<VeterinarioResponseDto>(resposta);
    }
}
