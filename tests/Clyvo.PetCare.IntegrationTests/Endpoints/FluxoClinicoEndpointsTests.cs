using System.Net;
using System.Net.Http.Json;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Endpoints;

/// <summary>
/// Jornada clínica completa: agendamento (atendente), evolução da consulta
/// (veterinário), retorno sugerido e carteira de vacinação. Padrão AAA.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class FluxoClinicoEndpointsTests : ApiTestBase
{
    public FluxoClinicoEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory) { }

    private async Task<(int PetId, int VeterinarioId)> PrepararCenarioAsync()
    {
        var admin = await ClienteAsync();
        var tutor = await CriarTutorAsync(admin);
        var pet = await CriarPetAsync(admin, tutor.Id);
        var veterinario = await CriarVeterinarioAsync(admin);
        return (pet.Id, veterinario.Id);
    }

    private static async Task<ConsultaResponseDto> AgendarAsync(HttpClient cliente, int petId, int veterinarioId, DateTime dataHora)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/consultas", new CriarConsultaDto
        {
            DataHora = dataHora, Tipo = TipoConsulta.Preventiva, MotivoConsulta = "Check-up", PetId = petId, VeterinarioId = veterinarioId
        });
        resposta.EnsureSuccessStatusCode();
        return await LerAsync<ConsultaResponseDto>(resposta);
    }

    [Fact]
    public async Task Agendar_ComoAtendente_DeveRetornar201ComStatusAgendada()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);

        // Act
        var consulta = await AgendarAsync(atendente, petId, veterinarioId, DateTime.Today.AddDays(2).AddHours(10));

        // Assert
        Assert.Equal(StatusConsulta.Agendada, consulta.Status);
        Assert.Contains(consulta.Links, l => l.Rel == "pet");
        Assert.Contains(consulta.Links, l => l.Rel == "veterinario");
    }

    [Fact]
    public async Task EvoluirConsulta_ComoAtendente_DeveRetornar403()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);
        var consulta = await AgendarAsync(atendente, petId, veterinarioId, DateTime.Today.AddHours(9));

        // Act
        var resposta = await atendente.PutAsJsonAsync($"/api/v1/consultas/{consulta.Id}", new AtualizarConsultaDto
        {
            DataHora = consulta.DataHora, Tipo = consulta.Tipo, Status = StatusConsulta.Realizada, Diagnostico = "Saudável"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task EvoluirConsulta_RealizadaSemDiagnostico_DeveRetornar422()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var veterinario = await ClienteAsync(PerfilUsuario.Veterinario);
        var consulta = await AgendarAsync(veterinario, petId, veterinarioId, DateTime.Today.AddHours(9));

        // Act
        var resposta = await veterinario.PutAsJsonAsync($"/api/v1/consultas/{consulta.Id}", new AtualizarConsultaDto
        {
            DataHora = consulta.DataHora, Tipo = consulta.Tipo, Status = StatusConsulta.Realizada
        });

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task EvoluirConsulta_RealizadaComRetorno_DeveAparecerNosRetornosProximosENaoPoderSerRemovida()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var veterinario = await ClienteAsync(PerfilUsuario.Veterinario);
        var consulta = await AgendarAsync(veterinario, petId, veterinarioId, DateTime.Today.AddHours(9));
        var admin = await ClienteAsync();

        // Act
        var evolucao = await veterinario.PutAsJsonAsync($"/api/v1/consultas/{consulta.Id}", new AtualizarConsultaDto
        {
            DataHora = consulta.DataHora,
            Tipo = consulta.Tipo,
            Status = StatusConsulta.Realizada,
            Diagnostico = "Dermatite alérgica",
            Prescricao = "Shampoo terapêutico",
            ProximoRetorno = DateTime.Today.AddDays(10),
            ValorCobrado = 220m
        });
        var retornos = await LerAsync<ColecaoDto<ConsultaResponseDto>>(await admin.GetAsync("/api/v1/consultas/retornos-proximos/15"));
        var remocao = await admin.DeleteAsync($"/api/v1/consultas/{consulta.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, evolucao.StatusCode);
        Assert.Contains(retornos.Itens, c => c.Id == consulta.Id);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, remocao.StatusCode);
    }

    [Fact]
    public async Task RetornosProximos_JanelaInvalida_DeveRetornar422()
    {
        // Arrange
        var cliente = await ClienteAsync();

        // Act
        var resposta = await cliente.GetAsync("/api/v1/consultas/retornos-proximos/0");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarConsultas_FiltroPorVeterinarioEStatus_DeveFiltrar()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var admin = await ClienteAsync();
        await AgendarAsync(admin, petId, veterinarioId, DateTime.Today.AddDays(1));
        await AgendarAsync(admin, petId, veterinarioId, DateTime.Today.AddDays(2));

        // Act
        var resposta = await admin.GetAsync($"/api/v1/consultas?veterinarioId={veterinarioId}&status=Agendada&ordenarPor=DataHora&direcao=Asc");

        // Assert
        var pagina = await LerAsync<PagedResultDto<ConsultaResponseDto>>(resposta);
        Assert.Equal(2, pagina.TotalRegistros);
        Assert.True(pagina.Itens[0].DataHora < pagina.Itens[1].DataHora);
    }

    [Fact]
    public async Task RegistrarVacina_ComoVeterinario_DeveAparecerNaCarteiraENasAtrasadas()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var veterinario = await ClienteAsync(PerfilUsuario.Veterinario);

        // Act
        var registro = await veterinario.PostAsJsonAsync("/api/v1/vacinas", new CriarVacinaDto
        {
            Nome = "V10",
            DataAplicacao = DateTime.Today.AddYears(-1),
            DataProximaDose = DateTime.Today.AddDays(-5),
            Lote = "V10-2025",
            PetId = petId,
            VeterinarioId = veterinarioId
        });
        var carteira = await LerAsync<ColecaoDto<VacinaResponseDto>>(await veterinario.GetAsync($"/api/v1/vacinas/pet/{petId}"));
        var atrasadas = await LerAsync<ColecaoDto<VacinaResponseDto>>(await veterinario.GetAsync("/api/v1/vacinas/atrasadas"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        Assert.Single(carteira.Itens);
        var atrasada = Assert.Single(atrasadas.Itens);
        Assert.True(atrasada.DoseAtrasada);
    }

    [Fact]
    public async Task RegistrarVacina_ComoAtendente_DeveRetornar403()
    {
        // Arrange
        var (petId, _) = await PrepararCenarioAsync();
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);

        // Act
        var resposta = await atendente.PostAsJsonAsync("/api/v1/vacinas", new CriarVacinaDto
        {
            Nome = "Antirrábica", DataAplicacao = DateTime.Today, PetId = petId
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task RemoverPet_ComHistoricoClinico_DeveRetornar422()
    {
        // Arrange
        var (petId, veterinarioId) = await PrepararCenarioAsync();
        var admin = await ClienteAsync();
        await AgendarAsync(admin, petId, veterinarioId, DateTime.Today.AddDays(1));

        // Act
        var resposta = await admin.DeleteAsync($"/api/v1/pets/{petId}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }
}
